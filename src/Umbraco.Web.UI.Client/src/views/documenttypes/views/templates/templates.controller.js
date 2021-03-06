/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.TemplatesController
 * @function
 *
 * @description
 * The controller for the content type editor templates sub view
 */
(function() {
    'use strict';

    function TemplatesController($scope, entityResource, contentTypeHelper, $routeParams) {

        /* ----------- SCOPE VARIABLES ----------- */

        var vm = this;

        vm.availableTemplates = [];
        vm.updateTemplatePlaceholder = false;
        vm.loadingTemplates = false;


        /* ---------- INIT ---------- */

        function onInit() {

            vm.loadingTemplates = true;

            entityResource.getAll("Template").then(function(templates){

                vm.availableTemplates = templates;

                // update placeholder template information on new doc types
                if (!$routeParams.notemplate && $scope.model.id === 0) {
                  vm.updateTemplatePlaceholder = true;
                  vm.availableTemplates = contentTypeHelper.insertTemplatePlaceholder(vm.availableTemplates);
                }

                vm.loadingTemplates = false;

            });

        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.DocumentType.TemplatesController", TemplatesController);
})();
