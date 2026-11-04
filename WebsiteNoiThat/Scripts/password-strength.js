// Issue #130: Kiem tra do manh mat khau cho cac trang nhap/doi mat khau
// (dang ky, doi mat khau, lay lai mat khau, them/sua nguoi dung o quan tri).
//
// Cach dung:
//   - Them attribute data-pw-strength vao input[type=password] can do do manh.
//   - Them data-pw-optional="true" neu de trong mat khau la hop le
//     (vi du trang Edit nguoi dung: de trong = giu mat khau cu).
//   - Nhung script sau cac input: <script src="~/Scripts/password-strength.js"></script>
// Khong phu thuoc jQuery.
(function () {
    'use strict';

    var MIN_LENGTH = 8;
    var MAX_LENGTH = 32;
    var RULE_TEXT = 'Mật khẩu phải có ít nhất 8 ký tự, bao gồm cả chữ cái và số.';

    function isValid(password) {
        if (!password || password.length < MIN_LENGTH || password.length > MAX_LENGTH) return false;
        return /[a-zA-Z]/.test(password) && /\d/.test(password);
    }

    // Diem do manh 0-4: du do dai, co chu + so, co chu hoa + thuong, co ky tu dac biet
    function scoreOf(password) {
        var s = 0;
        if (password.length >= MIN_LENGTH) s++;
        if (/[a-zA-Z]/.test(password) && /\d/.test(password)) s++;
        if (/[a-z]/.test(password) && /[A-Z]/.test(password)) s++;
        if (/[^a-zA-Z0-9]/.test(password)) s++;
        return s;
    }

    function levelOf(score) {
        if (score <= 1) return { label: 'Yếu', color: '#e74a3b' };
        if (score === 2) return { label: 'Trung bình', color: '#f6c23e' };
        if (score === 3) return { label: 'Khá', color: '#36b9cc' };
        return { label: 'Mạnh', color: '#1cc88a' };
    }

    function buildMeter(input) {
        var wrap = document.createElement('div');
        wrap.className = 'pw-strength';
        wrap.style.cssText = 'margin-top:6px;max-width:480px;';
        wrap.innerHTML =
            '<div class="pw-strength-bar" style="height:6px;background:#e3e6ea;border-radius:3px;overflow:hidden;">' +
            '<div class="pw-strength-fill" style="height:100%;width:0;transition:width .2s,background-color .2s;"></div>' +
            '</div>' +
            '<small class="pw-strength-text" style="color:#858796;"></small>';
        if (input.nextSibling) {
            input.parentNode.insertBefore(wrap, input.nextSibling);
        } else {
            input.parentNode.appendChild(wrap);
        }
        return wrap;
    }

    function attach(input) {
        if (input.getAttribute('data-pw-ready') === 'true') return;
        input.setAttribute('data-pw-ready', 'true');
        if (input.getAttribute('data-pw-optional') === 'true') {
            input.setAttribute('data-pw-optional', 'true');
        }
        input.setAttribute('autocomplete', 'new-password');

        var wrap = buildMeter(input);
        var fill = wrap.querySelector('.pw-strength-fill');
        var text = wrap.querySelector('.pw-strength-text');

        function evaluate() {
            var value = input.value;
            if (!value) {
                fill.style.width = '0';
                text.textContent = input.getAttribute('data-pw-optional') === 'true'
                    ? '(Để trống nếu không đổi mật khẩu)' : '';
                text.style.color = '#858796';
                return;
            }
            var lv = levelOf(scoreOf(value));
            fill.style.width = (lv.label === 'Mạnh' ? 100 : lv.label === 'Khá' ? 75 : lv.label === 'Trung bình' ? 50 : 25) + '%';
            fill.style.backgroundColor = lv.color;
            if (isValid(value)) {
                text.textContent = 'Độ mạnh: ' + lv.label;
                text.style.color = lv.color;
            } else {
                text.textContent = 'Chưa đạt yêu cầu — ' + RULE_TEXT;
                text.style.color = '#e74a3b';
            }
        }

        input.addEventListener('input', evaluate);
        input.addEventListener('keyup', evaluate);
        evaluate();

        // Chan submit neu mat khau khong dat chinh sach
        input.form && input.form.addEventListener('submit', function (e) {
            var value = input.value;
            if (value && !isValid(value)) {
                e.preventDefault();
                input.focus();
                alert(RULE_TEXT);
            }
        });
    }

    function attachAll(root) {
        var inputs = (root || document).querySelectorAll('input[type=password][data-pw-strength]');
        for (var i = 0; i < inputs.length; i++) attach(inputs[i]);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () { attachAll(document); });
    } else {
        attachAll(document);
    }

    window.PasswordStrength = { attach: attach, attachAll: attachAll, isValid: isValid };
})();
