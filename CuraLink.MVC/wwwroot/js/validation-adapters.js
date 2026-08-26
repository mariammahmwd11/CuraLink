// Register the 'requiredtrue' validator adapter with jQuery Unobtrusive Validation.
// This enables client-side validation for the [RequiredTrue] attribute.

if ($.validator && $.validator.addMethod) {
    $.validator.addMethod('requiredtrue', function (value, element) {
        return $(element).is(':checked');
    });

    $.validator.unobtrusive.adapters.addBool('requiredtrue');
}
