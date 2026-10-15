(function () {
  'use strict';

  angular.module('quillingApp')
    .component('contactUsForm', {
      bindings: {
        onSubmit: '&',
      },
      templateUrl: 'app/components/contact-us-form/contact-us-form.template.html',
      controller: [function () {
        const $ctrl = this;

        $ctrl.$onInit = function () {
          $ctrl.form = {
            name: '',
            company: '',
            mobileNo: '',
            email: '',
            message: '',
            informed: false,
          };
        };

        $ctrl.submit = function () {
          $ctrl.onSubmit({ values: angular.copy($ctrl.form) });
        };
      }],
    });
})();
