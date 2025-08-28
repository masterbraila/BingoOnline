$(document).ready(function() {
    // Accordion logic: only load the form for the active tab, clear others
    $('#authAccordion').on('show.bs.collapse', function (e) {
        var target = $(e.target).attr('id');
        // Always clear all form containers before loading a new one
        $('#login-step-body').empty();
        $('#register-step-body').empty();
        $('#forgot-step-body').empty();
        setTimeout(function() {
            if (target === 'collapseOne') {
                $('#login-step-body').html('<div class="text-center text-muted">Loading login form...</div>');
                $('#login-step-body').load('/Identity/Account/Login?layout=none', function(response, status, xhr) {
                    if (status == "error") {
                        $('#login-step-body').html('<div class="text-danger">Failed to load login form: ' + xhr.status + ' ' + xhr.statusText + '</div>');
                        console.error('Login form load error:', xhr);
                    } else {
                        if (!response.trim()) {
                            $('#login-step-body').html('<div class="text-danger">Login form is empty. Check server response and authentication requirements.</div>');
                            console.error('Login form response was empty.');
                        }
                        attachLoginAjax();
                    }
                });
            } else if (target === 'collapseTwo') {
                $('#register-step-body').html('<div class="text-center text-muted">Loading registration form...</div>');
                $('#register-step-body').load('/Identity/Account/Register?layout=none', function(response, status, xhr) {
                    if (status == "error") {
                        $('#register-step-body').html('<div class="text-danger">Failed to load registration form: ' + xhr.status + ' ' + xhr.statusText + '</div>');
                        console.error('Register form load error:', xhr);
                    } else {
                        if (!response.trim()) {
                            $('#register-step-body').html('<div class="text-danger">Registration form is empty. Check server response and authentication requirements.</div>');
                            console.error('Register form response was empty.');
                        }
                        attachRegisterAjax();
                        if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
                            window.jQuery.validator.unobtrusive.parse($('#register-step-body'));
                        }
                    }
                });
            } else if (target === 'collapseThree') {
                $('#forgot-step-body').html('<div class="text-center text-muted">Loading password recovery form...</div>');
                $('#forgot-step-body').load('/Identity/Account/ForgotPassword?layout=none', function(response, status, xhr) {
                    if (status == "error") {
                        $('#forgot-step-body').html('<div class="text-danger">Failed to load password recovery form: ' + xhr.status + ' ' + xhr.statusText + '</div>');
                        console.error('Forgot password form load error:', xhr);
                    } else {
                        if (!response.trim()) {
                            $('#forgot-step-body').html('<div class="text-danger">Password recovery form is empty. Check server response and authentication requirements.</div>');
                            console.error('Forgot password form response was empty.');
                        }
                        attachForgotAjax();
                        if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
                            window.jQuery.validator.unobtrusive.parse($('#forgot-step-body'));
                        }
                    }
                });
            }
        }, 50); // Small delay to ensure DOM is cleared
    });
    // Trigger default tab (login) on modal open and force-load login form
    $('#authModal').on('show.bs.modal', function () {
        $('#collapseOne').collapse('show');
        // Explicitly trigger the show event handler logic for login tab
        $('#login-step-body').empty();
        $('#register-step-body').empty();
        $('#forgot-step-body').empty();
        setTimeout(function() {
            $('#login-step-body').html('<div class="text-center text-muted">Loading login form...</div>');
            $('#login-step-body').load('/Identity/Account/Login?layout=none', function(response, status, xhr) {
                if (status == "error") {
                    $('#login-step-body').html('<div class="text-danger">Failed to load login form: ' + xhr.status + ' ' + xhr.statusText + '</div>');
                    console.error('Login form load error:', xhr);
                } else {
                    if (!response.trim()) {
                        $('#login-step-body').html('<div class="text-danger">Login form is empty. Check server response and authentication requirements.</div>');
                        console.error('Login form response was empty.');
                    }
                    attachLoginAjax();
                }
            });
        }, 50);
    });
    // Only clear forms when modal is fully closed
    $('#authModal').on('hidden.bs.modal', function () {
        $('#login-step-body').html('');
        $('#register-step-body').html('');
        $('#forgot-step-body').html('');
    });
    // Attach AJAX login handler after login form is loaded
    function attachLoginAjax() {
        var $form = $('#login-step-body').find('form#account');
        if ($form.length === 0) return;
        $form.off('submit').on('submit', function(e) {
            e.preventDefault();
            var $btn = $form.find('button[type=submit]');
            $btn.prop('disabled', true);
            $.ajax({
                url: $form.attr('action') || '/Identity/Account/Login?layout=none',
                type: 'POST',
                data: $form.serialize(),
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                success: function(data) {
                    if (data && data.success) {
                        $('#authModal').modal('hide');
                        window.location.href = data.redirect || '/';
                    } else if (typeof data === 'string' && data.indexOf('form') !== -1) {
                        // Login failed, re-render form with validation
                        $('#login-step-body').html(data);
                        attachLoginAjax();
                    } else {
                        $('#login-step-body .text-danger').remove(); // Remove previous errors
                        $('#login-step-body').prepend('<div class="text-danger">Login failed. Please try again.</div>');
                        $btn.prop('disabled', false);
                    }
                },
                error: function(xhr) {
                    $('#login-step-body .text-danger').remove(); // Remove previous errors
                    $('#login-step-body').prepend('<div class="text-danger">Login failed. Please try again.</div>');
                    $btn.prop('disabled', false);
                }
            });
        });
    }
    // Attach AJAX register handler after register form is loaded
    function attachRegisterAjax() {
        var $form = $('#register-step-body').find('form#registerForm');
        if ($form.length === 0) return;
        $form.off('submit').on('submit', function(e) {
            e.preventDefault();
            var $btn = $form.find('button[type=submit]');
            $btn.prop('disabled', true);
            $.ajax({
                url: $form.attr('action') || '/Identity/Account/Register?layout=none',
                type: 'POST',
                data: $form.serialize(),
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                success: function(data) {
                    if (data && data.success) {
                        $('#authModal').modal('hide');
                        window.location.href = data.redirect || '/';
                    } else if (typeof data === 'string' && data.indexOf('form') !== -1) {
                        // Register failed, re-render form with validation
                        $('#register-step-body').html(data);
                        attachRegisterAjax();
                        if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
                            window.jQuery.validator.unobtrusive.parse($('#register-step-body'));
                        }
                    } else {
                        $('#register-step-body .text-danger').remove();
                        $('#register-step-body').prepend('<div class="text-danger">Registration failed. Please try again.</div>');
                        $btn.prop('disabled', false);
                    }
                },
                error: function(xhr) {
                    $('#register-step-body .text-danger').remove();
                    $('#register-step-body').prepend('<div class="text-danger">Registration failed. Please try again.</div>');
                    $btn.prop('disabled', false);
                }
            });
        });
    }
    // Attach AJAX forgot password handler after forgot form is loaded
    function attachForgotAjax() {
        var $form = $('#forgot-step-body').find('form#forgotForm');
        if ($form.length === 0) return;
        $form.off('submit').on('submit', function(e) {
            e.preventDefault();
            var $btn = $form.find('button[type=submit]');
            $btn.prop('disabled', true);
            $.ajax({
                url: $form.attr('action') || '/Identity/Account/ForgotPassword?layout=none',
                type: 'POST',
                data: $form.serialize(),
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                success: function(data) {
                    if (data && data.success) {
                        $('#forgot-step-body').html('<div class="alert alert-success">If your email is in our database, you will receive a password reset link.</div>');
                    } else if (typeof data === 'string' && data.indexOf('form') !== -1) {
                        // Forgot failed, re-render form with validation
                        $('#forgot-step-body').html(data);
                        attachForgotAjax();
                        if (window.jQuery && window.jQuery.validator && window.jQuery.validator.unobtrusive) {
                            window.jQuery.validator.unobtrusive.parse($('#forgot-step-body'));
                        }
                    } else {
                        $('#forgot-step-body .text-danger').remove();
                        $('#forgot-step-body').prepend('<div class="text-danger">Password reset failed. Please try again.</div>');
                        $btn.prop('disabled', false);
                    }
                },
                error: function(xhr) {
                    $('#forgot-step-body .text-danger').remove();
                    $('#forgot-step-body').prepend('<div class="text-danger">Password reset failed. Please try again.</div>');
                    $btn.prop('disabled', false);
                }
            });
        });
    }
});
