// wwwroot/js/atlas-theme.js

(function () {
    'use strict';

    // ========================================================================
    // Particle Background — Blue Palette
    // ========================================================================
    function initParticles() {
        const container = document.getElementById('particles');
        if (!container) return;

        const particleCount = 35;

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.classList.add('atlas-particle');

            particle.style.left = Math.random() * 100 + '%';

            const size = Math.random() * 3 + 1;
            particle.style.width = size + 'px';
            particle.style.height = size + 'px';

            const duration = Math.random() * 18 + 12;
            particle.style.animationDuration = duration + 's';
            particle.style.animationDelay = Math.random() * duration + 's';

            // Blue palette particles
            const colors = [
                'rgba(45, 127, 249, 0.5)',   // Primary blue
                'rgba(0, 210, 255, 0.4)',     // Cyan accent
                'rgba(99, 102, 241, 0.35)',   // Indigo
                'rgba(91, 160, 255, 0.3)',    // Light blue
                'rgba(255, 255, 255, 0.15)'   // White sparkle
            ];
            const color = colors[Math.floor(Math.random() * colors.length)];
            particle.style.background = color;
            particle.style.boxShadow = '0 0 ' + (size * 3) + 'px ' + color;

            container.appendChild(particle);
        }
    }

    // ========================================================================
    // Navbar Scroll Effect
    // ========================================================================
    function initNavbarScroll() {
        const navbar = document.getElementById('mainNavbar');
        if (!navbar) return;

        window.addEventListener('scroll', function () {
            if (window.pageYOffset > 50) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
        }, { passive: true });
    }

    // ========================================================================
    // Scroll Animations (Intersection Observer)
    // ========================================================================
    function initScrollAnimations() {
        const observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('atlas-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        });

        document.querySelectorAll('.atlas-fade-in').forEach(function (el) {
            observer.observe(el);
        });
    }

    // ========================================================================
    // Toast Auto-dismiss
    // ========================================================================
    function initToasts() {
        document.querySelectorAll('.atlas-toast').forEach(function (toast) {
            setTimeout(function () {
                toast.style.transition = 'opacity 300ms ease, transform 300ms ease';
                toast.style.opacity = '0';
                toast.style.transform = 'translateY(-10px)';
                setTimeout(function () { toast.remove(); }, 300);
            }, 5000);
        });
    }

    // ========================================================================
    // Button Ripple Effect
    // ========================================================================
    function initRippleEffect() {
        // Add keyframes
        if (!document.getElementById('atlas-ripple-styles')) {
            const style = document.createElement('style');
            style.id = 'atlas-ripple-styles';
            style.textContent = '@keyframes ripple { to { transform: scale(2.5); opacity: 0; } }';
            document.head.appendChild(style);
        }

        document.addEventListener('click', function (e) {
            const btn = e.target.closest('.atlas-btn');
            if (!btn) return;

            const rect = btn.getBoundingClientRect();
            const ripple = document.createElement('span');
            const size = Math.max(rect.width, rect.height);

            ripple.style.cssText =
                'position:absolute;width:' + size + 'px;height:' + size + 'px;' +
                'left:' + (e.clientX - rect.left - size / 2) + 'px;' +
                'top:' + (e.clientY - rect.top - size / 2) + 'px;' +
                'background:rgba(255,255,255,0.12);border-radius:50%;' +
                'transform:scale(0);animation:ripple 600ms ease-out;' +
                'pointer-events:none;z-index:1;';

            btn.style.position = 'relative';
            btn.style.overflow = 'hidden';
            btn.appendChild(ripple);

            setTimeout(function () { ripple.remove(); }, 600);
        });
    }

    // ========================================================================
    // Smooth page fade-in
    // ========================================================================
    function initPageTransition() {
        document.body.style.opacity = '0';
        document.body.style.transition = 'opacity 300ms ease';

        window.addEventListener('load', function () {
            requestAnimationFrame(function () {
                document.body.style.opacity = '1';
            });
        });
    }

    function initValidationStyles() {
        const inputs = document.querySelectorAll('.atlas-input');
        if (!inputs.length) return;

        const observer = new MutationObserver((mutations) => {
            for (const m of mutations) {
                if (m.type !== 'attributes' || m.attributeName !== 'class') continue;

                const el = m.target;
                if (!el.classList || !el.classList.contains('atlas-input')) continue;

                const shouldHaveError = el.classList.contains('input-validation-error');
                const hasError = el.classList.contains('atlas-input--error');

                // Gereksiz add/remove yapma
                if (shouldHaveError === hasError) continue;

                // Kendi class değişikliğimizin tekrar tetiklenmesini engelle
                observer.disconnect();
                if (shouldHaveError) el.classList.add('atlas-input--error');
                else el.classList.remove('atlas-input--error');

                // Tüm inputları tekrar observe et
                inputs.forEach(i => observer.observe(i, { attributes: true, attributeFilter: ['class'] }));
            }
        });

        inputs.forEach((input) => {
            observer.observe(input, { attributes: true, attributeFilter: ['class'] });

            // İlk yükleme
            if (input.classList.contains('input-validation-error') &&
                !input.classList.contains('atlas-input--error')) {
                input.classList.add('atlas-input--error');
            }
        });

        // Summary boşsa gizle (ama sonradan dolarsa görünür olsun)
        document.querySelectorAll('.atlas-validation-summary').forEach((summary) => {
            const update = () => {
                const list = summary.querySelector('ul');
                const empty = !list || list.children.length === 0;
                summary.style.display = empty ? 'none' : '';
            };
            update();

            // jQuery validation summary sonradan değişirse takip et
            const sumObs = new MutationObserver(update);
            sumObs.observe(summary, { childList: true, subtree: true });
        });
    }


    // ========================================================================
    // Init
    // ========================================================================

    function init() {
        initPageTransition();
        initParticles();
        initNavbarScroll();
        initScrollAnimations();
        initToasts();
        initRippleEffect();
        initValidationStyles()
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();