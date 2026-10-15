(function () {
  'use strict';

  angular.module('quillingApp')
    .component('checkoutPage', {
      templateUrl: 'app/pages/checkout/checkout.template.html',
      controller: ['$window', 'CartService', 'CheckoutService', function ($window, CartService, CheckoutService) {
        const $ctrl = this;

        $ctrl.$onInit = function () {
          $ctrl.cart = CartService;
          $ctrl.loading = false;
          $ctrl.error = '';
        };

        $ctrl.pay = function () {
          if (CartService.isEmpty() || $ctrl.loading) return;

          $ctrl.loading = true;
          $ctrl.error = '';

          const origin = $window.location.origin;
          CheckoutService.createSession(
            CartService.items(),
            `${origin}/checkout/success?session_id={CHECKOUT_SESSION_ID}`,
            `${origin}/cart`
          )
            .then(session => {
              // Leave the SPA for the Stripe-hosted checkout page (sandbox in test mode).
              $window.location.assign(session.url);
            })
            .catch(err => {
              $ctrl.error = err.message;
              $ctrl.loading = false;
            });
        };
      }],
    });
})();
