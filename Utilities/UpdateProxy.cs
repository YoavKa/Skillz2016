using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBot.Utilities
{
    /// <summary>
    /// A proxy, which updates all it's members at update
    /// </summary>
    class UpdateProxy<T> : IUpdateable<T>
    {
        //////////Attributes//////////

        /// <summary>
        /// The members to update at update
        /// </summary>
        private List<IUpdateable<T>> members;



        //////////Methods//////////

        /// <summary>
        /// Creates an empty UpdateProxy
        /// </summary>
        public UpdateProxy()
        {
            this.members = new List<IUpdateable<T>>();
        }

        /// <summary>
        /// Adds a member to the proxy
        /// </summary>
        public void Add(IUpdateable<T> member)
        {
            if (member == null)
                return;
            this.members.Add(member);
        }

        /// <summary>
        /// Removes a member from the proxy
        /// </summary>
        /// <param name="member">The member to remove</param>
        /// <returns>True if the member was successfully removed</returns>
        public bool Remove(IUpdateable<T> member)
        {
            return this.members.Remove(member);
        }

        /// <summary>
        /// Updates all the members of the proxy
        /// </summary>
        public void Update(T t)
        {
            foreach (IUpdateable<T> member in this.members)
                member.Update(t);
        }
    }



    /// <summary>
    /// An interface for updateable objects
    /// </summary>
    /// <typeparam name="T">The type of object the Update method gets as an argument</typeparam>
    interface IUpdateable<T>
    {
        /// <summary>
        /// Updates the object, based on the object given
        /// </summary>
        void Update(T t);
    }
}
