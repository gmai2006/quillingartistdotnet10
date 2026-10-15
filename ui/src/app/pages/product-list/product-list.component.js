(function () {
  'use strict';

  // Product grid for one category (/ shows "Animal", /category/:cat the rest).
  angular.module('quillingApp')
    .component('productListPage', {
      bindings: {
        products: '<',
      },
      templateUrl: 'app/pages/product-list/product-list.template.html',
      controller: ['$routeParams', function ($routeParams) {
        const $ctrl = this;

        $ctrl.$onChanges = function () {
          const cat = $routeParams.cat || 'Animal';
          $ctrl.filtered = ($ctrl.products || []).filter(item => item.category === cat);
        };
      }],
    });
})();
