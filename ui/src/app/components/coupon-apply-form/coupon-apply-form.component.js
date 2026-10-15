(function () {
  'use strict';

  angular.module('quillingApp')
    .component('couponApplyForm', {
      bindings: {
        onSubmit: '&',
      },
      templateUrl: 'app/components/coupon-apply-form/coupon-apply-form.template.html',
      controller: [function () {
        const $ctrl = this;

        $ctrl.$onInit = function () {
          $ctrl.coupon = '';
        };

        $ctrl.submit = function () {
          $ctrl.onSubmit({ values: { coupon: $ctrl.coupon } });
        };
      }],
    });
})();
