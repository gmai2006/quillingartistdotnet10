(function () {
  'use strict';

  angular.module('quillingApp')
    .component('topMenu', {
      templateUrl: 'app/components/top-menu/top-menu.template.html',
      controller: ['CATEGORIES', function (CATEGORIES) {
        const $ctrl = this;

        $ctrl.$onInit = function () {
          $ctrl.categories = CATEGORIES;
          $ctrl.isOpen = false;
        };

        $ctrl.toggleMenu = function () {
          $ctrl.isOpen = !$ctrl.isOpen;
        };
      }],
    });
})();
