(function () {
  'use strict';

  angular.module('quillingApp')
    .component('cartPage', {
      templateUrl: 'app/pages/cart/cart.template.html',
      controller: ['$window', 'CartService', function ($window, CartService) {
        const $ctrl = this;

        $ctrl.$onInit = function () {
          $ctrl.cart = CartService;
        };

        // Placeholder, as in the React UI: coupons are not implemented yet.
        $ctrl.applyCoupon = function (values) {
          $window.alert(JSON.stringify(values));
        };
      }],
    });
})();
