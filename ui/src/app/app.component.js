(function () {
  'use strict';

  // Root shell (port of App.jsx): header, category menu, routed view, footer.
  angular.module('quillingApp')
    .component('quillingApp', {
      templateUrl: 'app/app.template.html',
      controller: ['ProductService', function (ProductService) {
        const $ctrl = this;

        $ctrl.$onInit = function () {
          // Show the spinner until the catalog is loaded once; routes then
          // resolve products from the cached promise without a second request.
          $ctrl.loading = true;
          ProductService.getAll().finally(() => { $ctrl.loading = false; });
        };
      }],
    });
})();
