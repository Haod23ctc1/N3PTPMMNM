// password-toggle.js
// Thu vien dung chung: them nut "con mat" cho moi o input[type=password]
// - Tu dong bao lai o mat khau trong the .pw-field (position: relative)
// - Bam nut: doi type password <-> text va doi icon mat
// - Tranh trang login tu co san nut (dung lop .password-field thi bo qua)
// Khong phu thuoc jQuery; dung FontAwesome da co san tren toan bo site.
(function () {
    'use strict';

    var cssId = 'pw-toggle-style';
    if (!document.getElementById(cssId)) {
        var style = document.createElement('style');
        style.id = cssId;
        style.textContent = '.pw-field{position:relative;}' +
            '.pw-toggle{position:absolute;right:10px;top:50%;transform:translateY(-50%);' +
            'background:transparent;border:none;color:#6c757d;padding:0;line-height:1;cursor:pointer;z-index:5;}' +
            '.pw-toggle:hover{color:#007bff;}' +
            '.pw-field input{padding-right:34px;}' +
            '.pw-toggle .fa-eye-slash{color:#007bff;}';
        document.head.appendChild(style);
    }

    function enhance(input) {
        if (input.getAttribute('data-pw-enhanced') === '1') return;
        // Trang dang nhap co san nut rieng roi (.password-field + .toggle-password)
        if (input.parentElement && input.parentElement.classList &&
            input.parentElement.classList.contains('password-field')) return;
        input.setAttribute('data-pw-enhanced', '1');

        var wrap = document.createElement('div');
        wrap.className = 'pw-field position-relative';
        if (input.parentNode) {
            input.parentNode.insertBefore(wrap, input);
            wrap.appendChild(input);

            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'pw-toggle';
            btn.setAttribute('aria-label', 'Hien/an mat khau');
            btn.title = 'Hiển thị mật khẩu';
            btn.innerHTML = '<i class="fa fa-eye" aria-hidden="true"></i>';
            btn.addEventListener('click', function () {
                var isPw = input.getAttribute('type') === 'password';
                input.setAttribute('type', isPw ? 'text' : 'password');
                btn.innerHTML = '<i class="fa ' + (isPw ? 'fa-eye-slash' : 'fa-eye') + '" aria-hidden="true"></i>';
                btn.title = isPw ? 'Ẩn mật khẩu' : 'Hiển thị mật khẩu';
            });
            wrap.appendChild(btn);
        }
    }

    function run() {
        var inputs = document.querySelectorAll('input[type=password]');
        for (var i = 0; i < inputs.length; i++) enhance(inputs[i]);
    }

    if (document.readyState !== 'loading') {
        run();
    } else {
        document.addEventListener('DOMContentLoaded', run);
    }
})();
