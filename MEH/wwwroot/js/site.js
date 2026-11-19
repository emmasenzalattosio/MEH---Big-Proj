// THEME TOGGLE (Dark / Light Mode)
(function () {
    function applyTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        const sun = document.getElementById('themeIconSun');
        const moon = document.getElementById('themeIconMoon');
        if (sun && moon) {
            if (theme === 'light') {
                sun.classList.remove('d-none');
                moon.classList.add('d-none');
            } else {
                sun.classList.add('d-none');
                moon.classList.remove('d-none');
            }
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        const toggleBtn = document.getElementById('themeToggleBtn');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', function () {
                const current = document.documentElement.getAttribute('data-bs-theme') || 'dark';
                const next = current === 'dark' ? 'light' : 'dark';
                applyTheme(next);
                localStorage.setItem('theme', next);
            });
        }
        const currentTheme = document.documentElement.getAttribute('data-bs-theme') || 'dark';
        applyTheme(currentTheme);
        const bgVideo = document.querySelector('.sd-bg-video');
        if (bgVideo) { bgVideo.playbackRate = 0.75; }
    });
})();
