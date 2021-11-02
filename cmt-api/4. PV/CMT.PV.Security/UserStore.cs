using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace CMT.PV.Security
{
    /// <summary>
    ///     EntityFramework based user store implementation that supports IUserStore, IUserLoginStore, IUserClaimStore and
    ///     IUserRoleStore
    /// </summary>
    public class UserStore :
        //IUserLoginStore<UserBO, Guid>,
        //IUserClaimStore<UserBO, Guid>,
        IUserRoleStore<UserBO, Guid>,
        IUserPasswordStore<UserBO, Guid>,
        IUserSecurityStampStore<UserBO, Guid>,
        //IQueryableUserStore<UserBO, Guid>,
        IUserEmailStore<UserBO, Guid>,
        IUserPhoneNumberStore<UserBO, Guid>,
        IUserTwoFactorStore<UserBO, Guid>,
        IUserLockoutStore<UserBO, Guid>
    {
        //private readonly IDbSet<UserLoginBO> _logins;
        protected RoleManager _roleStore;
        //private readonly IDbSet<UserClaimBO> _userClaims;
        private UserRoleManager _userRoles;
        private bool _disposed;
        protected UserManager _userStore;
        public const string AdminRoleName = "admin";

        /// <summary>
        ///     Constructor which takes a db context and wires up the stores with default instances using the context
        /// </summary>
        /// <param name="context"></param>
        public UserStore(DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            Context = context;
            AutoSaveChanges = true;
            _userStore = new UserManager();
            _roleStore = new RoleManager();
            //_logins = Context.Set<UserLoginBO>();
            //_userClaims = Context.Set<UserClaimBO>();
            _userRoles = new UserRoleManager();
        }

        /// <summary>
        ///     Context for the store
        /// </summary>
        public DbContext Context { get; private set; }

        /// <summary>
        ///     If true will call dispose on the DbContext during Dispose
        /// </summary>
        public bool DisposeContext { get; set; }

        /// <summary>
        ///     If true will call SaveChanges after Create/Update/Delete
        /// </summary>
        public bool AutoSaveChanges { get; set; }

        ///// <summary>
        /////     Returns an IQueryable of users
        ///// </summary>
        //public IQueryable<UserBO> Users
        //{
        //    get { throw new NotImplementedException(); }
        //}

        ///// <summary>
        /////     Return the claims for a user
        ///// </summary>
        ///// <param name="user"></param>
        ///// <returns></returns>
        //public virtual async Task<IList<Claim>> GetClaimsAsync(UserBO user)
        //{
        //    ThrowIfDisposed();
        //    if (user == null)
        //    {
        //        throw new ArgumentNullException("user");
        //    }
        //    await EnsureClaimsLoaded(user).WithCurrentCulture();
        //    return user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
        //}

        ///// <summary>
        /////     Add a claim to a user
        ///// </summary>
        ///// <param name="user"></param>
        ///// <param name="claim"></param>
        ///// <returns></returns>
        //public virtual Task AddClaimAsync(UserBO user, Claim claim)
        //{
        //    ThrowIfDisposed();
        //    if (user == null)
        //    {
        //        throw new ArgumentNullException("user");
        //    }
        //    if (claim == null)
        //    {
        //        throw new ArgumentNullException("claim");
        //    }
        //    _userClaims.Add(new UserClaimBO { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value });
        //    return Task.FromResult(0);
        //}

        ///// <summary>
        /////     Remove a claim from a user
        ///// </summary>
        ///// <param name="user"></param>
        ///// <param name="claim"></param>
        ///// <returns></returns>
        //public virtual async Task RemoveClaimAsync(UserBO user, Claim claim)
        //{
        //    ThrowIfDisposed();
        //    if (user == null)
        //    {
        //        throw new ArgumentNullException("user");
        //    }
        //    if (claim == null)
        //    {
        //        throw new ArgumentNullException("claim");
        //    }
        //    IEnumerable<UserClaimBO> claims;
        //    var claimValue = claim.Value;
        //    var claimType = claim.Type;
        //    if (AreClaimsLoaded(user))
        //    {
        //        claims = user.Claims.Where(uc => uc.ClaimValue == claimValue && uc.ClaimType == claimType).ToList();
        //    }
        //    else
        //    {
        //        var userId = user.Id;
        //        claims = await _userClaims.Where(uc => uc.ClaimValue == claimValue && uc.ClaimType == claimType && uc.UserId.Equals(userId)).ToListAsync().WithCurrentCulture();
        //    }
        //    foreach (var c in claims)
        //    {
        //        _userClaims.Remove(c);
        //    }
        //}

        /// <summary>
        ///     Returns whether the user email is confirmed
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<bool> GetEmailConfirmedAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.EmailConfirmed);
        }

        /// <summary>
        ///     Set IsConfirmed on the user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <returns></returns>
        public virtual Task SetEmailConfirmedAsync(UserBO user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Set the user email
        /// </summary>
        /// <param name="user"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public virtual Task SetEmailAsync(UserBO user, string email)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.Email = email;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Get the user's email
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<string> GetEmailAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.Email);
        }

        /// <summary>
        ///     Find a user by email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public virtual Task<UserBO> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            return GetUserAggregateAsync(u => u.Email.ToUpper() == email.ToUpper());
        }

        /// <summary>
        ///     Returns the DateTimeOffset that represents the end of a user's lockout, any time in the past should be considered
        ///     not locked out.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<DateTimeOffset> GetLockoutEndDateAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return
                Task.FromResult(user.LockoutEndDateUtc.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                    : new DateTimeOffset());
        }

        /// <summary>
        ///     Locks a user out until the specified end date (set to a past date, to unlock a user)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="lockoutEnd"></param>
        /// <returns></returns>
        public virtual Task SetLockoutEndDateAsync(UserBO user, DateTimeOffset lockoutEnd)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue ? (DateTime?)null : lockoutEnd.UtcDateTime;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Used to record when an attempt to access the user has failed
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<int> IncrementAccessFailedCountAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
        ///     Used to reset the account access count, typically after the account is successfully accessed
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task ResetAccessFailedCountAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Returns the current number of failed access attempts.  This number usually will be reset whenever the password is
        ///     verified or the account is locked out.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<int> GetAccessFailedCountAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
        ///     Returns whether the user can be locked out.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<bool> GetLockoutEnabledAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.LockoutEnabled);
        }

        /// <summary>
        ///     Sets whether the user can be locked out.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public virtual Task SetLockoutEnabledAsync(UserBO user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Find a user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public virtual Task<UserBO> FindByIdAsync(Guid userId)
        {
            ThrowIfDisposed();
            return GetUserAggregateAsync(u => u.Id.Equals(userId));
        }

        /// <summary>
        ///     Find a user by name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public virtual Task<UserBO> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            return GetUserAggregateAsync(u => u.UserName.ToUpper() == userName.ToUpper());
        }

        /// <summary>
        ///     Insert an entity
        /// </summary>
        /// <param name="user"></param>
        public virtual async Task CreateAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            _userStore.InsertObject(user);
            await SaveChanges().WithCurrentCulture();
        }

        /// <summary>
        ///     Mark an entity for deletion
        /// </summary>
        /// <param name="user"></param>
        public virtual async Task DeleteAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            _userStore.DeleteObject(user);
            await SaveChanges().WithCurrentCulture();
        }

        /// <summary>
        ///     Update an entity
        /// </summary>
        /// <param name="user"></param>
        public virtual async Task UpdateAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            _userStore.UpdateObject(user);
            await SaveChanges().WithCurrentCulture();
        }

        /// <summary>
        ///     Dispose the store
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Set the password hash for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="passwordHash"></param>
        /// <returns></returns>
        public virtual Task SetPasswordHashAsync(UserBO user, string passwordHash)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Get the password hash for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<string> GetPasswordHashAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.PasswordHash);
        }

        /// <summary>
        ///     Returns true if the user has a password set
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<bool> HasPasswordAsync(UserBO user)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        /// <summary>
        ///     Set the user's phone number
        /// </summary>
        /// <param name="user"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public virtual Task SetPhoneNumberAsync(UserBO user, string phoneNumber)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Get a user's phone number
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<string> GetPhoneNumberAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.PhoneNumber);
        }

        /// <summary>
        ///     Returns whether the user phoneNumber is confirmed
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<bool> GetPhoneNumberConfirmedAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        /// <summary>
        ///     Set PhoneNumberConfirmed on the user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <returns></returns>
        public virtual Task SetPhoneNumberConfirmedAsync(UserBO user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }


        /// <summary>
        ///     Get the names of the roles a user is a member of
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual async Task<IList<string>> GetRolesAsync(UserBO user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return await _userRoles.GetUserRoles(user.Id);
            //return null;
            //ThrowIfDisposed();
            //if (user == null)
            //{
            //    throw new ArgumentNullException("user");
            //}
            //var userId = user.Id;
            //var query = from userRole in _userRoles
            //            where userRole.UserId.Equals(userId)
            //            join role in _roleStore.DbEntitySet on userRole.RoleId equals role.Id
            //            select role.Name;
            //return await query.ToListAsync().WithCurrentCulture();
        }

        /// <summary>
        ///     Returns true if the user is in the named role
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public virtual async Task<bool> IsInRoleAsync(UserBO user, string roleName)
        {

            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Role Name cannot be null", "roleName");
            }

            return await _userRoles.IsUserInRole(user.Id, roleName);
        }

        /// <summary>
        ///     Returns true if the user is in the named role
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public virtual async Task<bool> IsInRoleAsync(Guid userId, string roleName)
        {

            if (userId == Guid.Empty)
            {
                throw new ArgumentException("Cannot be Guid.Empty", nameof(userId));
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Role Name cannot be null", nameof(roleName));
            }

            return await _userRoles.IsUserInRole(userId, roleName);
        }

        /// <summary>
        ///     Set the security stamp for the user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="stamp"></param>
        /// <returns></returns>
        public virtual Task SetSecurityStampAsync(UserBO user, string stamp)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Get the security stamp for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<string> GetSecurityStampAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.SecurityStamp);
        }

        /// <summary>
        ///     Set whether two factor authentication is enabled for the user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public virtual Task SetTwoFactorEnabledAsync(UserBO user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Gets whether two factor authentication is enabled for the user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Task<bool> GetTwoFactorEnabledAsync(UserBO user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.TwoFactorEnabled);
        }

        // Only call save changes if AutoSaveChanges is true
        private async Task SaveChanges()
        {
            if (AutoSaveChanges)
            {
                await Context.SaveChangesAsync().WithCurrentCulture();
            }
        }

        //private bool AreClaimsLoaded(UserBO user)
        //{
        //    return Context.Entry(user).Collection(u => u.Claims).IsLoaded;
        //}

        //private async Task EnsureClaimsLoaded(UserBO user)
        //{
        //    if (!AreClaimsLoaded(user))
        //    {
        //        var userId = user.Id;
        //        await _userClaims.Where(uc => uc.UserId.Equals(userId)).LoadAsync().WithCurrentCulture();
        //        Context.Entry(user).Collection(u => u.Claims).IsLoaded = true;
        //    }
        //}

        //private async Task EnsureRolesLoaded(UserBO user)
        //{
        //    if (!Context.Entry(user).Collection(u => u.Roles).IsLoaded)
        //    {
        //        var userId = user.Id;
        //        await _userRoles.Where(uc => uc.UserId.Equals(userId)).LoadAsync().WithCurrentCulture();
        //        Context.Entry(user).Collection(u => u.Roles).IsLoaded = true;
        //    }
        //}

        //private bool AreLoginsLoaded(UserBO user)
        //{
        //    return Context.Entry(user).Collection(u => u.Logins).IsLoaded;
        //}

        //private async Task EnsureLoginsLoaded(UserBO user)
        //{
        //    if (!AreLoginsLoaded(user))
        //    {
        //        var userId = user.Id;
        //        await _logins.Where(uc => uc.UserId.Equals(userId)).LoadAsync().WithCurrentCulture();
        //        Context.Entry(user).Collection(u => u.Logins).IsLoaded = true;
        //    }
        //}

        /// <summary>
        /// Used to attach child entities to the User aggregate, i.e. Roles, Logins, and Claims
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        protected virtual async Task<UserBO> GetUserAggregateAsync(Expression<Func<UserBO, bool>> filter)
        {
            Guid id;
            UserBO user;
            if (FindByIdFilterParser.TryMatchAndGetId(filter, out id))
            {
                user = await _userStore.GetObjectByIdAsync(id).WithCurrentCulture();
            }
            else
            {
                user = await Task.FromResult(_userStore.GetObjectsUsingBOPredicate(filter).FirstOrDefault()).WithCurrentCulture();
            }
            //if (user != null)
            //{
            //    await EnsureClaimsLoaded(user).WithCurrentCulture();
            //    await EnsureLoginsLoaded(user).WithCurrentCulture();
            //    await EnsureRolesLoaded(user).WithCurrentCulture();
            //}
            return user;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <summary>
        ///     If disposing, calls dispose on the Context.  Always nulls out the Context
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            Context?.Database?.Connection?.Close();

            if (DisposeContext && disposing)
            {
                Context?.Dispose();
            }

            _disposed = true;
            Context = null;
            _userStore = null;
        }

        public Task AddToRoleAsync(UserBO user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromRoleAsync(UserBO user, string roleName)
        {
            throw new NotImplementedException();
        }

        // We want to use FindAsync() when looking for an User.Id instead of LINQ to avoid extra 
        // database roundtrips. This class cracks open the filter expression passed by 
        // UserStore.FindByIdAsync() to obtain the value of the id we are looking for 
        private static class FindByIdFilterParser
        {
            // expression pattern we need to match
            private static readonly Expression<Func<UserBO, bool>> Predicate = u => u.Id.Equals(default(Guid));
            // method we need to match: Object.Equals() 
            private static readonly MethodInfo EqualsMethodInfo = ((MethodCallExpression)Predicate.Body).Method;
            // property access we need to match: User.Id 
            private static readonly MemberInfo UserIdMemberInfo = ((MemberExpression)((MethodCallExpression)Predicate.Body).Object).Member;

            internal static bool TryMatchAndGetId(Expression<Func<UserBO, bool>> filter, out Guid id)
            {
                // default value in case we can’t obtain it 
                id = default(Guid);

                // lambda body should be a call 
                if (filter.Body.NodeType != ExpressionType.Call)
                {
                    return false;
                }

                // actually a call to object.Equals(object)
                MethodCallExpression callExpression = (MethodCallExpression)filter.Body;
                if (callExpression.Method != EqualsMethodInfo)
                {
                    return false;
                }
                // left side of Equals() should be an access to User.Id
                if (callExpression.Object == null
                    || callExpression.Object.NodeType != ExpressionType.MemberAccess
                    || ((MemberExpression)callExpression.Object).Member != UserIdMemberInfo)
                {
                    return false;
                }

                // There should be only one argument for Equals()
                if (callExpression.Arguments.Count != 1)
                {
                    return false;
                }

                MemberExpression fieldAccess;
                if (callExpression.Arguments[0].NodeType == ExpressionType.Convert)
                {
                    // convert node should have an member access access node
                    // This is for cases when primary key is a value type
                    UnaryExpression convert = (UnaryExpression)callExpression.Arguments[0];
                    if (convert.Operand.NodeType != ExpressionType.MemberAccess)
                    {
                        return false;
                    }
                    fieldAccess = (MemberExpression)convert.Operand;
                }
                else if (callExpression.Arguments[0].NodeType == ExpressionType.MemberAccess)
                {
                    // Get field member for when key is reference type
                    fieldAccess = (MemberExpression)callExpression.Arguments[0];
                }
                else
                {
                    return false;
                }

                // and member access should be a field access to a variable captured in a closure
                if (fieldAccess.Member.MemberType != MemberTypes.Field
                    || fieldAccess.Expression.NodeType != ExpressionType.Constant)
                {
                    return false;
                }

                // expression tree matched so we can now just get the value of the id 
                FieldInfo fieldInfo = (FieldInfo)fieldAccess.Member;
                object closure = ((ConstantExpression)fieldAccess.Expression).Value;

                id = (Guid)fieldInfo.GetValue(closure);
                return true;
            }
        }
    }
}
