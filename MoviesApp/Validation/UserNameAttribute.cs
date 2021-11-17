using System.ComponentModel.DataAnnotations;

namespace MoviesApp.Validation
{
    public class UserNameAttribute : ValidationAttribute
    {
        private string name;
        private readonly int length;

        public UserNameAttribute(int length)
        {
            this.length = length;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value.ToString()?.Length < length)
            {
                return new ValidationResult($"The length of the name must be greater than {length - 1}!");
            }

            return ValidationResult.Success;
        }
    }
}