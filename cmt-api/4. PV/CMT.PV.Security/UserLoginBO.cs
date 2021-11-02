using System;

namespace CMT.PV.Security
{
    /// <summary>
    ///     Business object for a user's login (i.e. facebook, google)
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public partial class UserLoginBO : UserLoginBO<Guid>
    {
    }

    /// <summary>
    ///     Business object for a user's login (i.e. facebook, google)
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class UserLoginBO<TKey>
    {
        /// <summary>
        ///     The login provider for the login (i.e. facebook, google)
        /// </summary>
        public virtual string LoginProvider { get; set; }

        /// <summary>
        ///     Key representing the login for the provider
        /// </summary>
        public virtual string ProviderKey { get; set; }

        /// <summary>
        ///     Display name for the login
        /// </summary>
        public virtual string ProviderDisplayName { get; set; }

        /// <summary>
        ///     User Id for the user who owns this login
        /// </summary>
        public virtual TKey UserId { get; set; }
    }
}