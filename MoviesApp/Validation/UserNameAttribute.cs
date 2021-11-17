using System.ComponentModel.DataAnnotations;

namespace MoviesApp.Validation
{
    public class UserNameAttribute : ValidationAttribute
    {
        private readonly int _length;

        public UserNameAttribute(int length)
        {
            _length = length;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value.ToString()?.Length < _length)
            {
                return new ValidationResult($"The length of the name must be greater than {_length - 1}!");
            }

            return ValidationResult.Success;
        }
    }
}