import boto3, sys, gzip, os, csv, ntpath, time, uuid, logging
from optparse import OptionParser, make_option


class FileReaderHelper():

    def __init__(self, file_path, is_file_gzipped=False):
                
        self.is_file_gzipped = is_file_gzipped
        self.file_path = file_path
        self.file_handle = None

        if self.is_file_gzipped:
            self.file_handle = gzip.open(file_path, 'rt', encoding='UTF8')
        else:
            self.file_handle = open(file_path, 'rt', encoding='UTF8')

    def __enter__(self):
        return self.file_handle
    def __exit__(self, type, value, traceback):
        self.close()
        
    def close(self):
        self.file_handle.close()

class MCMDataWriter(object):
    def __init__(self, working_directory, output_file_name):
        self.working_directory = working_directory
        self.output_file_name = output_file_name
        self.file_name_extension = 'csv'
        self.write_time = int(round(time.time() * 1000))
        self.colum_for_split = {}
        self.file_headers = []
        self.files_handlers = {}

    def __enter__(self):
        return self
    def __exit__(self, type, value, traceback):
        self.close()
        

    def setFileHeaders(self, headers):
        if isinstance(headers, list):
            self.file_headers = headers
        else:
            raise ValueError('Invalid parameter type. Expected type is a list.')

    def setColumnForSplit(self, column_name):
        for i, column in enumerate(self.file_headers):
            if column == column_name:
                self.colum_for_split = {"name":column_name, "index": i}
                break


    def registerNewFile(self,file_id):
        if file_id not in self.files_handlers:
            file = open(self.getFileName(file_id),'w+', encoding='UTF8')
            csv_writer = csv.writer(file, delimiter="|", quotechar='"', quoting=csv.QUOTE_MINIMAL, lineterminator='\n')
            self.files_handlers[file_id] = {"file_id":file_id, "file_obj":file, "csv_writer": csv_writer, "processed_rows": 0}
            csv_writer.writerow(self.file_headers)

    def getFileName(self, file_prefix):
        return "{}\{}_{}_{}.{}".format(self.working_directory, file_prefix, self.output_file_name, str(self.write_time), self.file_name_extension)

    def add_rows(self, rows):

        #list(map(self.process_row, rows))
        for row in rows:
            self.process_row(row)


    def process_row(self,row):
        split_column_value = row[self.colum_for_split["index"]]
        if split_column_value not in self.files_handlers:
            self.registerNewFile(split_column_value)
         
        self.files_handlers[split_column_value]["csv_writer"].writerow(row)
        self.files_handlers[split_column_value]["processed_rows"] = self.files_handlers[split_column_value]["processed_rows"] + 1
        if self.files_handlers[split_column_value]["processed_rows"] % 10000 == 0:
            file_object = self.files_handlers[split_column_value]["file_obj"]
            file_object.flush()
            os.fsync(file_object)

    def getOutputFiles(self):
        data = {k: v["file_obj"].name for k, v in self.files_handlers.items()}
        return data


    def close(self):
        for handler_name in self.files_handlers:
            file_obj = self.files_handlers[handler_name]["file_obj"]
            if file_obj != None:
                if not file_obj.closed:
                    file_obj.close()

def main(argv):


    logger.info("Process started")
    
    option_list = [
        make_option("-w", "--localWorkingDir",
               action="store", type="string", dest="local_working_directory", default=os.getcwd()),
        make_option("--sfs", "--srcfilesep",
               action="store", type="string", dest="src_file_delimiter", default=','),
        make_option("--sfqc", "--srcfileqoutechar",
               action="store", type="string", dest="src_file_quotechar", default='"'),
        make_option("--sak", "--srcs3accesskey",
               action="store", type="string", dest="src_aws_access_key_id"),
        make_option("--ssk", "--srcs3secretkey",
               action="store", type="string", dest="src_aws_secret_access_key"),
        make_option("--dak", "--dsts3accesskey",
               action="store", type="string", dest="dst_aws_access_key_id"),
        make_option("--dsk", "--dsts3secretkey",
               action="store", type="string", dest="dst_aws_secret_access_key"),
        make_option("--sbn", "--srcs3bucket",
               action="store", type="string", dest="src_s3_bucket"),
        make_option("--sfk", "--srcs3filekey",
               action="store", type="string", dest="src_s3_file_key"),
        make_option("--dbn", "--dsts3bucket",
               action="store", type="string", dest="dst_s3_bucket"),
        make_option("--dfk", "--dsts3filekey",
               action="store", type="string", dest="dst_s3_file_key"),
        make_option("--sgz", "--srcgzip",
               action="store_true", dest="src_is_file_gzipped", default=False)
    ]

    parser = OptionParser(option_list=option_list)

    (options, args) = parser.parse_args()

    if  not (options.src_aws_access_key_id and options.src_aws_secret_access_key and options.src_s3_bucket and options.src_s3_file_key and options.dst_s3_bucket and options.dst_s3_file_key):
        parser.error("Required parameters are missing. You can view help to find required parameters.")
  

    if not (options.dst_aws_access_key_id and options.dst_aws_secret_access_key):
       options.dst_aws_access_key_id = options.src_aws_access_key_id
       options.dst_aws_secret_access_key = options.src_aws_secret_access_key
    


    src_file_name = options.src_s3_file_key.split('/')[-1].replace(".csv.gz", "")   

    #Downloading file from S3
    st = time.time()
    logger.info("Downloading file from S3 - started")
    temp_file = downloadFileFromS3(options.local_working_directory, options.src_aws_access_key_id, options.src_aws_secret_access_key, options.src_s3_bucket, options.src_s3_file_key)
    #temp_file = "C:\DW_MCM_EPSILON_DATA_RAW_20161228.csv.gz"
    logger.info("Downloading file from S3 - completed. Elapsed time {0:.1f}sek.".format(time.time() - st))

    #File splitting
    st = time.time()
    logger.info("Splitting file by country column values - started")
    files_to_upload = splitMCMDataFile(options.local_working_directory, temp_file, src_file_name, options.src_file_delimiter, options.src_file_quotechar)
    logger.info("Splitting by country column values - completed. Elapsed time {0:.1f}sek.".format(time.time() - st))

    #Uploading files to S3
    st = time.time()
    logger.info("Uploading files to S3 - started")
    uploadFilesToS3(files_to_upload, options.dst_aws_access_key_id, options.dst_aws_secret_access_key, options.dst_s3_bucket, options.dst_s3_file_key)
    logger.info("Uploadinf files to S3 - completed. Elapsed time {0:.1f}sek.".format(time.time() - st))

    #Removing files
    st = time.time()
    logger.info("Removing local temporary files - started")
    os.remove(temp_file)
    for file in files_to_upload.items():
        os.remove(file[1])
    logger.info("Removing local temporary files - completed. Elapsed time {0:.1f}sek.".format(time.time() - st))

def downloadFileFromS3(working_directory, aws_access_key_id, aws_secret_access_key, bucket, file_key):

    client = boto3.client(
        's3',
        aws_access_key_id = aws_access_key_id,
        aws_secret_access_key = aws_secret_access_key)
    logger.info(os.path.join(working_directory, str(uuid.uuid4())))
    temp_file_path = os.path.join(working_directory, str(uuid.uuid4()))
    temp_file_path_gz = temp_file_path + '.gz'
            
    client.download_file(bucket, file_key, temp_file_path_gz) 
    logger.info("return file name")
    return temp_file_path_gz

def uploadFilesToS3(files_to_upload, aws_access_key_id, aws_secret_access_key, bucket, key_prefix):
    
    client = boto3.client(
        's3',
        aws_access_key_id = aws_access_key_id,
        aws_secret_access_key = aws_secret_access_key)
    
    countries = {
"ARGENTINA":"AR",
"AUSTRALIA":"AU",
"AUSTRIA":"AT",
"BARBADOS":"BB",
"BELGIUM":"BE",
"BRAZIL":"BR",
"CHILE":"CL",
"COLOMBIA":"CO",
"COLUMBIA":"CO",
"COSTA RICA":"CR",
"DENMARK":"DK",
"DOMINICAN REPUBLIC":"DO",
"EGYPT":"AG",
"EL SALVADOR":"SV",
"FINLAND":"FI",
"FRANCE":"FR",
"GERMANY":"DE",
"GLOBAL":"GL",
"GREECE":"GR",
"GUATEMALA":"GT",
"HONDURAS":"HN",
"INDIA":"IN",
"IRELAND":"IR",
"ITALY":"IT",
"KOREA":"KR",
"MALAYSIA":"MY",
"MEXICO":"MX",
"NETHERLAND":"NL",
"NETHERLANDS":"NL",
"NICARAGUA":"NI",
"PANAMA":"PA",
"PHILIPPINES":"PH",
"POLAND":"PL",
"PORTUGAL":"PT",
"RUSSIA":"RU",
"SAUDI ARABIA":"SA",
"SPAIN":"SP",
"SWITZERLAND":"CH",
"TAIWAN":"TW",
"TRINIDAD AND TOMBAGO":"TT",
"TURKEY":"TR",
"UK":"UK",
"UNITED KINGDOM":"UK",
"VENEZUELA":"VE"
}


    for file in files_to_upload.items():
        file_name =  ntpath.basename(file[1])
        file_folder= file[0]
        if file[0] in countries:
            file_folder = countries[file[0]]
        s3_file_key = "{}/{}/{}".format(key_prefix,file_folder, file_name)
        client.upload_file(file[1], bucket, s3_file_key)

def splitMCMDataFile(working_directory, input_file_path, src_file_name, delimiter=',', quotechar='"'):
    current_piece = 1
    row_limit = 10
    current_limit = row_limit
    row_buffer = []
    file_with_headers = True
    start_time = time.time()

    with FileReaderHelper(input_file_path, True) as file:
            reader = csv.reader(file, delimiter=delimiter, quotechar=quotechar, quoting=csv.QUOTE_MINIMAL)
            with MCMDataWriter(working_directory, src_file_name) as writer:
                if file_with_headers:
                    headers = next(reader)
                    writer.setFileHeaders(headers)
                    writer.setColumnForSplit("country")
                for i, row in enumerate(reader):
                    if i % 10000 == 0:
                        print('Processed rows = {}'.format(str(i)), end='')
                        print('\r' * len(str(i + 1)), end='') 
             
                    if i + 1 > current_limit:
                        current_piece += 1
                        current_limit = row_limit * current_piece
                        writer.add_rows(row_buffer)
                        row_buffer = []
                    row_buffer.append(row)
                #writing what have left in buffer
                writer.add_rows(row_buffer)
                files_to_upload = writer.getOutputFiles()

    processing_time = time.time() - start_time
    return files_to_upload

if __name__ == "__main__":
    
    logger = logging.getLogger('S3FileSplitter')
    formatter = logging.Formatter('%(asctime)s %(levelname)s %(message)s', "%Y-%m-%d %H:%M:%S")
    logger.setLevel(logging.INFO)

    hdlr_file = logging.FileHandler('S3FileSplitter.log')
    hdlr_file.setFormatter(formatter)
    logger.addHandler(hdlr_file) 

    log_stream = logging.StreamHandler()
    log_stream.setFormatter(formatter)
    logger.addHandler(log_stream)

    if 'boto3' not in sys.modules:
        raise ReferenceError("This script requires boto3 package. Please ensire if it is installed properly")
    if sys.version_info < (3, 4):
        raise "Script requires python verion 3.5 or higher"

    main(sys.argv)
