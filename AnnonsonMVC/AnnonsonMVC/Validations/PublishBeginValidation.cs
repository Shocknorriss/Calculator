﻿using AnnonsonMVC.ViewModels;
using System;
using System.ComponentModel.DataAnnotations;

namespace AnnonsonMVC.Validations
{
    public class PublishBeginValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ArticelViewModel articelViewModel = (ArticelViewModel)validationContext.ObjectInstance;

            if (articelViewModel.PublishBegin <= DateTime.Today)
            {

            }
            return ValidationResult.Success;
        }
    }
}