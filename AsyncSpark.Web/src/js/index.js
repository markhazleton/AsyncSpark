// All requires run in source order — no ES static imports (which are hoisted by webpack)
// This guarantees window.$ = jQuery before any plugin executes

const jQuery = require('jquery');
window.$ = window.jQuery = jQuery;

// jQuery 4 removed $.parseJSON — shim for jquery-validation-unobtrusive compatibility
jQuery.parseJSON = JSON.parse;

require('jquery-validation');
require('jquery-validation-unobtrusive');
require('datatables.net');
require('datatables.net-bs5');
const toastr = require('toastr');
require('toastr/build/toastr.min.css');

window.toastr = toastr;

toastr.options = {
    closeButton: true,
    progressBar: true,
    positionClass: 'toast-bottom-right',
    timeOut: 5000
};

require('./site');
require('./async-stats');
const AsyncStatsDashboard = require('./async-stats-dashboard').default;

jQuery(function () {
    jQuery('.table').DataTable();

    if (document.getElementById('async-stats-container')) {
        new AsyncStatsDashboard('async-stats-container');
    }
});
