﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ASC.Utilities.CustomValidationAttributes
{
    public class FutureDateAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly int _days;
        private readonly string _errorMessage = "Date cannot be after {0} days from current date.";
        private FutureDateAttribute() { }
        public FutureDateAttribute(int days)
        {
            _days = days;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var date = (DateTime)value;
            if (date > DateTime.UtcNow.AddDays(_days))
                return new ValidationResult(string.Format(_errorMessage, _days));
            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val-futuredate", string.Format(_errorMessage, _days));
            context.Attributes.Add("data-val-futuredate-days", _days.ToString());
        }
    }
}
