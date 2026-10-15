(function () {
  'use strict';

  angular.module('quillingApp')
    .component('documentationPage', {
      templateUrl: 'app/pages/documentation/documentation.template.html',
      controller: [function () {
        const $ctrl = this;

        $ctrl.$onInit = function () {
          $ctrl.tabs = [
            { id: 'introduction', label: 'Introduction', icon: 'BookOpen' },
            { id: 'requirements', label: 'System Requirements', icon: 'Monitor' },
            { id: 'installation', label: 'Installation', icon: 'Download' },
            { id: 'screenshots', label: 'Screenshots', icon: 'Image' },
          ];
          $ctrl.activeTab = 'introduction';
        };

        $ctrl.activeLabel = function () {
          return $ctrl.tabs.find(tab => tab.id === $ctrl.activeTab).label;
        };
      }],
    });
})();
