/**
@ngdoc directive
@name umbraco.directives.directive:umbButton
@restrict E
@scope

@description
Use this directive to render an umbraco button. The directive can be used to generate all types of buttons, set type, style, translation, shortcut and much more.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <umb-button
            action="vm.clickButton()"
            type="button"
            button-style="success"
            state="vm.buttonState"
            shortcut="ctrl+c"
            label="My button"
            disabled="vm.buttonState === 'busy'">
        </umb-button>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function Controller(myService) {

            var vm = this;
            vm.buttonState = "init";

            vm.clickButton = clickButton;

            function clickButton() {

                vm.buttonState = "busy";

                myService.clickButton().then(function() {
                    vm.buttonState = "success";
                }, function() {
                    vm.buttonState = "error";
                });

            }
        }

        angular.module("umbraco").controller("My.Controller", Controller);

    })();
</pre>

@param {callback} action The button action which should be performed when the button is clicked.
@param {string=} href Url/Path to navigato to.
@param {string=} type Set the button type ("button" or "submit").
@param {string=} buttonStyle Set the style of the button. The directive uses the default bootstrap styles ("primary", "info", "success", "warning", "danger", "inverse", "link", "block"). Pass in array to add multple styles [success,block].
@param {string=} state Set a progress state on the button ("init", "busy", "success", "error").
@param {string=} shortcut Set a keyboard shortcut for the button ("ctrl+c").
@param {string=} label Set the button label.
@param {string=} labelKey Set a localization key to make a multi lingual button ("general_buttonText").
@param {string=} icon Set a button icon.
@param {string=} size Set a button icon ("xs", "m", "l", "xl").
@param {boolean=} disabled Set to <code>true</code> to disable the button.
**/

(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbButton', {
            transclude: true,
            templateUrl: 'views/components/buttons/umb-button.html',
            controller: UmbButtonController,
            controllerAs: 'vm',
            bindings: {
                action: "&?",
                href: "@?",
                type: "@",
                buttonStyle: "@?",
                state: "<?",
                shortcut: "@?",
                shortcutWhenHidden: "@",
                label: "@?",
                labelKey: "@?",
                icon: "@?",
                disabled: "<?",
                size: "@?",
                alias: "@?"
            }
        });

    UmbButtonController.$inject = ['$timeout'];

    function UmbButtonController($timeout) {

        var vm = this;

        vm.$onInit = onInit;
        vm.$onChanges = onChanges;
        vm.clickButton = clickButton;

        function onInit() {

            vm.blockElement = false;
            vm.style = null;
            vm.innerState = "init";

            if (vm.buttonStyle) {

                // make it possible to pass in multiple styles
                if(vm.buttonStyle.startsWith("[") && vm.buttonStyle.endsWith("]")) {
                    
                    // when using an attr it will always be a string so we need to remove square brackets
                    // and turn it into and array
                    var withoutBrackets = vm.buttonStyle.replace(/[\[\]']+/g,'');
                    // split array by , + make sure to catch whitespaces
                    var array = withoutBrackets.split(/\s?,\s?/g);
                    
                    angular.forEach(array, function(item){
                        vm.style = vm.style + " " + "btn-" + item;
                        if(item === "block") {
                            vm.blockElement = true;
                        }
                    });

                } else {
                    vm.style = "btn-" + vm.buttonStyle;
                    if(vm.buttonStyle === "block") {
                        vm.blockElement = true;
                    }
                }

            }

        }

        function onChanges(changes) {

            // watch for state changes
            if (changes.state) {
                if(changes.state.currentValue) {
                    vm.innerState = changes.state.currentValue;
                }
                if (changes.state.currentValue === 'success' || changes.state.currentValue === 'error') {
                    // set the state back to 'init' after a success or error 
                    $timeout(function () {
                        vm.innerState = 'init';
                    }, 2000);
                }
            }

            // watch for disabled changes
            if(changes.disabled) {
                if(changes.disabled.currentValue) {
                    vm.disabled = changes.disabled.currentValue;
                }
            }
        }

        function clickButton(event) {
            if(vm.action) {
                vm.action({$event: event});
            }
        }

    }

})();
