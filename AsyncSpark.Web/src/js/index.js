// Import libraries
import jQuery from 'jquery';
window.$ = window.jQuery = jQuery;

// jQuery 4 removed $.parseJSON — shim for jquery-validation-unobtrusive compatibility
jQuery.parseJSON = JSON.parse;

// Use require() so these load after jQuery is assigned to window.$
// (ES import statements are hoisted and would run before the assignment above)
require('jquery-validation');
require('jquery-validation-unobtrusive');
require('datatables.net');
require('datatables.net-bs5');
const toastr = require('toastr');
require('toastr/build/toastr.min.css');

// Make toastr globally available
window.toastr = toastr;

// Configure toastr
toastr.options = {
    closeButton: true,
    progressBar: true,
    positionClass: 'toast-bottom-right',
    timeOut: 5000
};

// IMPORTANT: All custom scripts must be imported here to be included in the bundle
// ================================================================================
// NOTE: Bootstrap and theme switching are now handled by WebSpark.Bootswatch NuGet package

// Import custom scripts
import './site';                                     // Site-wide functionality
import './async-stats';                              // Async statistics tracking
import AsyncStatsDashboard from './async-stats-dashboard'; // Dashboard visualization

// Initialize components on document ready
jQuery(function () {
    // Initialize DataTables
    jQuery('.table').DataTable();
    
    // Initialize async stats dashboard if container exists
    if (document.getElementById('async-stats-container')) {
        new AsyncStatsDashboard('async-stats-container');
    }
});