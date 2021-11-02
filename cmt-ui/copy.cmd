move dist\main*.js dist\cmt.script.js
move dist\scripts.*.bundle.js dist\cmt.scripts.bundle.js
move dist\styles*.js dist\cmt.css.js
move dist\styles*.bundle.css dist\cmt.css

copy dist\*.js "\\cf-vm-iis-e\RDM_DEV\Scripts\"
copy dist\*.css "\\cf-vm-iis-e\RDM_DEV\Styles\"
#copy src\testSchemas-data.json "..\..\RDM\CFR_RRDMSite\"
#mkdir "..\..\RDM\CFR_RRDMSite\Assets\Images"
copy src\assets\images\*.* ""\\cf-vm-iis-e\RDM_DEV\Assets\Images"