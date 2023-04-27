namespace ASP_201MVC.Models.User
{
    public class ProfileModel
    {
        public Guid Id { get; set; }

        public String Name { get; set; }

        public String Login { get; set; }

        public String Email { get; set; }

        public String Avatar { get; set; }

        public DateTime RegisterDt { get; set; }

        public bool isDatetimePublic { get; set; }

        public bool IsNamePublic { get; set; }

        public bool IsEmailPublic { get; set; }

        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        /// Чи є даний профіль персональним(для конкретного юзера)
        /// </summary>
        public bool isPersonal { get; set; }


        public ProfileModel(Data.Entity.User user)
        {
            var thisProps = this.GetType().GetProperties();
            foreach (var prop in user.GetType().GetProperties())
            {
                var thisProp = thisProps.FirstOrDefault(p => 
                p.Name == prop.Name && p.PropertyType.IsAssignableFrom(prop.PropertyType));
                if (thisProp is not null)
                {
                    thisProp?.SetValue(this, prop.GetValue(user));
                }
                this.IsEmailConfirmed = user.EmailCode is null;
            }
        }
    }
}
