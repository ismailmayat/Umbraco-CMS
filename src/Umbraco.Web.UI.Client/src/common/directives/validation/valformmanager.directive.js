﻿/**
* @ngdoc directive
* @name umbraco.directives.directive:valFormManager
* @restrict A
* @require formController
* @description Used to broadcast an event to all elements inside this one to notify that form validation has 
* changed. If we don't use this that means you have to put a watch for each directive on a form's validation
* changing which would result in much higher processing. We need to actually watch the whole $error collection of a form
* because just watching $valid or $invalid doesn't acurrately trigger form validation changing.
* This also sets the show-validation (or a custom) css class on the element when the form is invalid - this lets
* us css target elements to be displayed when the form is submitting/submitted.
* Another thing this directive does is to ensure that any .control-group that contains form elements that are invalid will
* be marked with the 'error' css class. This ensures that labels included in that control group are styled correctly.
**/
function valFormManager(serverValidationManager, $rootScope, $log, $timeout, notificationsService, eventsService, $routeParams) {

    var SHOW_VALIDATION_CLASS_NAME = "show-validation";
    var SAVING_EVENT_NAME = "formSubmitting";
    var SAVED_EVENT_NAME = "formSubmitted";

    return {
        require: "form",
        restrict: "A",
        controller: function($scope) {
            //This exposes an API for direct use with this directive

            var unsubscribe = [];
            var self = this;

            //This is basically the same as a directive subscribing to an event but maybe a little
            // nicer since the other directive can use this directive's API instead of a magical event
            this.onValidationStatusChanged = function (cb) {
                unsubscribe.push($scope.$on("valStatusChanged", function(evt, args) {
                    cb.apply(self, [evt, args]);
                }));
            };

            //Ensure to remove the event handlers when this instance is destroyted
            $scope.$on('$destroy', function () {
                for (var u in unsubscribe) {
                    unsubscribe[u]();
                }
            });
        },
        link: function (scope, element, attr, formCtrl) {

            //watch the list of validation errors to notify the application of any validation changes
            scope.$watch(function () {
                //the validators are in the $error collection: https://docs.angularjs.org/api/ng/type/form.FormController#$error
                //since each key is the validator name (i.e. 'required') we can't just watch the number of keys, we need to watch
                //the sum of the items inside of each key

                //get the lengths of each array for each key in the $error collection
                var validatorLengths = _.map(formCtrl.$error, function (val, key) {
                    return val.length;
                });
                //sum up all numbers in the resulting array
                var sum = _.reduce(validatorLengths, function (memo, num) {
                    return memo + num;
                }, 0);
                //this is the value we watch to notify of any validation changes on the form
                return sum;
            }, function (e) {
                scope.$broadcast("valStatusChanged", { form: formCtrl });
                
                //find all invalid elements' .control-group's and apply the error class
                var inError = element.find(".control-group .ng-invalid").closest(".control-group");
                inError.addClass("error");

                //find all control group's that have no error and ensure the class is removed
                var noInError = element.find(".control-group .ng-valid").closest(".control-group").not(inError);
                noInError.removeClass("error");

            });

            //This tracks if the user is currently saving a new item, we use this to determine 
            // if we should display the warning dialog that they are leaving the page - if a new item
            // is being saved we never want to display that dialog, this will also cause problems when there
            // are server side validation issues.
            var isSavingNewItem = false;

            //we should show validation if there are any msgs in the server validation collection
            if (serverValidationManager.items.length > 0) {
                element.addClass(SHOW_VALIDATION_CLASS_NAME);
            }

            var unsubscribe = [];

            //listen for the forms saving event
            unsubscribe.push(scope.$on(SAVING_EVENT_NAME, function(ev, args) {
                element.addClass(SHOW_VALIDATION_CLASS_NAME);

                //set the flag so we can check to see if we should display the error.
                isSavingNewItem = $routeParams.create;
            }));

            //listen for the forms saved event
            unsubscribe.push(scope.$on(SAVED_EVENT_NAME, function(ev, args) {
                //remove validation class
                element.removeClass(SHOW_VALIDATION_CLASS_NAME);

                //clear form state as at this point we retrieve new data from the server
                //and all validation will have cleared at this point    
                formCtrl.$setPristine();
            }));

            //This handles the 'unsaved changes' dialog which is triggered when a route is attempting to be changed but
            // the form has pending changes
            var locationEvent = $rootScope.$on('$locationChangeStart', function(event, nextLocation, currentLocation) {
                if (!formCtrl.$dirty || isSavingNewItem) {
                    return;
                }

                var path = nextLocation.split("#")[1];
                if (path) {
                    if (path.indexOf("%253") || path.indexOf("%252")) {
                        path = decodeURIComponent(path);
                    }

                    if (!notificationsService.hasView()) {
                        var msg = { view: "confirmroutechange", args: { path: path, listener: locationEvent } };
                        notificationsService.add(msg);
                    }

                    //prevent the route!
                    event.preventDefault();

                    //raise an event
                    eventsService.emit("valFormManager.pendingChanges", true);
                }

            });
            unsubscribe.push(locationEvent);

            //Ensure to remove the event handler when this instance is destroyted
            scope.$on('$destroy', function() {
                for (var u in unsubscribe) {
                    unsubscribe[u]();
                }
            });

            $timeout(function(){
                formCtrl.$setPristine();
            }, 1000);
            
        }
    };
}
angular.module('umbraco.directives.validation').directive("valFormManager", valFormManager);
