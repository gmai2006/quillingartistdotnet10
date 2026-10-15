(function () {
  'use strict';

  angular.module('quillingApp')
    .component('supportCard', {
      bindings: {
        icon: '@',
        iconClass: '@',
        heading: '@',
      },
      templateUrl: 'app/components/support-card/support-card.template.html',
      controller: [function () {
        const $ctrl = this;

        $ctrl.$onInit = function () {
          $ctrl.links = [
            'Cras justo odio',
            'Dapibus ac facilisis in',
            'Morbi leo risus',
            'Porta ac consectetur ac',
            'Vestibulum at eros',
          ];
        };
      }],
    });
})();
