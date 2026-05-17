const { Notyf } = require('notyf');
require('notyf/notyf.min.css');

// Create a global notyf instance with default options
window.notyf = new Notyf({
    duration: 5000,
    position: { x: 'right', y: 'bottom' },
    dismissible: true,
    types: [
        { type: 'info', background: '#0dcaf0', icon: false }
    ]
});

// Thin toastr-compatible shim so existing call sites need no changes
window.toastr = {
    success: (msg) => window.notyf.success(msg),
    error: (msg) => window.notyf.error(msg),
    info: (msg) => window.notyf.open({ type: 'info', message: msg }),
    options: {}
};

require('./site');
require('./async-stats');
const AsyncStatsDashboard = require('./async-stats-dashboard').default;

document.addEventListener('DOMContentLoaded', function () {
    if (document.getElementById('async-stats-container')) {
        new AsyncStatsDashboard('async-stats-container');
    }
});
