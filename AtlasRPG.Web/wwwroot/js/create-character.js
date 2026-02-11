(function () {
    'use strict';

    // ========================================================================
    // DOM
    // ========================================================================
    var btnSubmit = document.getElementById('btnSubmit');
    var summaryRace = document.getElementById('summaryRace');
    var summaryClass = document.getElementById('summaryClass');
    var raceStatus = document.getElementById('raceStatus');
    var classStatus = document.getElementById('classStatus');
    var form = document.getElementById('createForm');

    var raceInputs = document.querySelectorAll('.atlas-race-card__input');
    var classInputs = document.querySelectorAll('.atlas-class-card__input');

    // Build name maps
    var raceNames = {};
    document.querySelectorAll('.atlas-race-card').forEach(function (card) {
        var input = card.querySelector('.atlas-race-card__input');
        var name = card.querySelector('.atlas-race-card__name');
        if (input && name) raceNames[input.value] = name.textContent.trim();
    });

    var classNames = {};
    document.querySelectorAll('.atlas-class-card').forEach(function (card) {
        var input = card.querySelector('.atlas-class-card__input');
        var name = card.querySelector('.atlas-class-card__name');
        if (input && name) classNames[input.value] = name.textContent.trim();
    });

    // ========================================================================
    // State
    // ========================================================================
    var selectedRace = null;
    var selectedClass = null;

    function updateSubmitState() {
        btnSubmit.disabled = !(selectedRace && selectedClass);
    }

    // ========================================================================
    // Race Selection
    // ========================================================================
    raceInputs.forEach(function (input) {
        input.addEventListener('change', function () {
            selectedRace = this.value;
            var name = raceNames[selectedRace] || 'Unknown';

            // Update bottom bar
            summaryRace.textContent = name;
            summaryRace.classList.remove('atlas-create-submit-bar__value--empty');
            summaryRace.classList.add('atlas-create-submit-bar__value--filled');

            // Update section header badge
            raceStatus.innerHTML =
                '<span class="atlas-badge atlas-badge--primary atlas-badge--selected">' +
                '<i class="fas fa-check-circle"></i> ' + name +
                '</span>';

            updateSubmitState();

            // Auto-scroll to class section if class not yet selected
            if (!selectedClass) {
                setTimeout(function () {
                    var classSection = document.querySelector('.atlas-section-icon--class');
                    if (classSection) {
                        classSection.closest('.atlas-create-section').scrollIntoView({
                            behavior: 'smooth',
                            block: 'start'
                        });
                    }
                }, 250);
            }
        });
    });

    // ========================================================================
    // Class Selection
    // ========================================================================
    classInputs.forEach(function (input) {
        input.addEventListener('change', function () {
            selectedClass = this.value;
            var name = classNames[selectedClass] || 'Unknown';

            // Update bottom bar
            summaryClass.textContent = name;
            summaryClass.classList.remove('atlas-create-submit-bar__value--empty');
            summaryClass.classList.add('atlas-create-submit-bar__value--filled');

            // Update section header badge
            classStatus.innerHTML =
                '<span class="atlas-badge atlas-badge--accent atlas-badge--selected">' +
                '<i class="fas fa-check-circle"></i> ' + name +
                '</span>';

            updateSubmitState();
        });
    });

    // ========================================================================
    // Form Submit — prevent double click
    // ========================================================================
    if (form) {
        form.addEventListener('submit', function (e) {
            if (!selectedRace || !selectedClass) {
                e.preventDefault();
                return;
            }

            btnSubmit.disabled = true;
            btnSubmit.innerHTML =
                '<i class="fas fa-spinner fa-spin"></i> Creating Champion...';
        });
    }

})();