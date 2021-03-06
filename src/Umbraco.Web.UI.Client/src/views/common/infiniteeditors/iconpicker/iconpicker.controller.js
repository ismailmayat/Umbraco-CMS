/**
 * @ngdoc controller
 * @name Umbraco.Editors.IconPickerController
 * @function
 *
 * @description
 * The controller for the content type editor icon picker
 */
function IconPickerController($scope, iconHelper, localizationService) {

    var vm = this;

    vm.selectIcon = selectIcon;
    vm.close = close;

    function onInit() {

        vm.loading = true;

        setTitle();
    
        iconHelper.getIcons().then(function (icons) {
            vm.icons = icons;
            vm.loading = false;
        });

    }

    function setTitle() {
        if (!$scope.model.title) {
            localizationService.localize("defaultdialogs_selectIcon")
                .then(function(data){
                    $scope.model.title = data;
                });
        }
    }

    function selectIcon(icon, color) {
        $scope.model.icon = icon;
        $scope.model.color = color;
        submit();
    }

    function close() {
        if($scope.model && $scope.model.close) {
            $scope.model.close();
        }
    }

    function submit() {
        if($scope.model && $scope.model.submit) {
            $scope.model.submit($scope.model);
        }
    }

    onInit();

}

angular.module("umbraco").controller("Umbraco.Editors.IconPickerController", IconPickerController);
