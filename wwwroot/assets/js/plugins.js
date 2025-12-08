// Load optional plugins only when needed - using CDN fallbacks
(function() {
    var hasToast = document.querySelectorAll("[toast-list]").length > 0;
    var hasChoices = document.querySelectorAll("[data-choices]").length > 0;
    var hasDatepicker = document.querySelectorAll("[data-provider]").length > 0;
    
    if (hasToast) {
        document.writeln("<script type='text/javascript' src='https://cdn.jsdelivr.net/npm/toastify-js'><\/script>");
    }
    if (hasChoices) {
        document.writeln("<script type='text/javascript' src='/assets/libs/choices.js/public/assets/scripts/choices.min.js'><\/script>");
    }
    if (hasDatepicker) {
        document.writeln("<script type='text/javascript' src='/assets/libs/flatpickr/flatpickr.min.js'><\/script>");
    }
})();