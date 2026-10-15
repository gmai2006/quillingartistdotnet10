(function () {
  'use strict';

  angular.module('quillingApp')
    .component('appFooter', {
      templateUrl: 'app/components/app-footer/app-footer.template.html',
      controller: ['CATEGORIES', function (CATEGORIES) {
        const $ctrl = this;

        $ctrl.$onInit = function () {
          // The React footer shuffled then sorted the first six categories,
          // which is simply the first six, sorted.
          $ctrl.categories = CATEGORIES.slice(0, 6).sort();
        };
      }],
    });
})();
