(function () {
  'use strict';

  angular.module('quillingApp')
    .component('contactUsPage', {
      templateUrl: 'app/pages/contact-us/contact-us.template.html',
      controller: ['$window', function ($window) {
        const $ctrl = this;

        $ctrl.$onInit = function () {
          $ctrl.offices = ['Head Office', 'Development Office'];
        };

        // Placeholder, as in the React UI: there is no contact endpoint yet.
        $ctrl.send = function (values) {
          $window.alert(JSON.stringify(values));
        };
      }],
    });
})();
