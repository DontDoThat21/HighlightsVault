// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Highlight initial date calendar selector addition box setup script
// Set Highlight Date to current date
try {
    document.getElementById("highlightDate").value = new Date().toISOString().slice(0, 10);
} catch (error) {
    console.error('Error setting highlightDate value:', error);
}
// Infinite scroll functionality
var tableContainer = document.getElementById('tableContainer');
tableContainer.addEventListener('scroll', function () {
    if (tableContainer.scrollTop + tableContainer.clientHeight >= tableContainer.scrollHeight - 10) {
        loadMoreHighlights();
    }
});

// AJAX to load more paginated views when scrolling reaches last row/highlight.
function loadMoreHighlights() {
    currentPage++;
    isLoading = true;
    fetch('/Highlights/HighlightsVault?page=' + currentPage, {
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => response.text())
        .then(data => {
            var tableBody = document.getElementById("highlightsBody");
            tableBody.insertAdjacentHTML('beforeend', data);
            isLoading = false;
        })
        .catch(error => {
            console.error('Error loading more data:', error);
            isLoading = false;
        });
}

var isFiltersVisible = true;
var currentPage = 1;

document.addEventListener('DOMContentLoaded', function () {
    var filtersRow = document.getElementById('filtersRow'); // Ensure 'filtersRow' matches the ID of the row you want to hide
    
});

// Function to toggle the visibility of the row based on the value of isFiltersVisible
function toggleRowVisibility() {
    var filtersRow = document.getElementById('filtersRow');
    var filterButton = document.getElementById('filterButton');

    var filterName = document.getElementById('nameFilter');
    var filterSteamId = document.getElementById('steamIdFilter');
    var filterUserDescription = document.getElementById('userDescriptionFilter');
    var filterHighlightDate = document.getElementById('highlightDateFilter');


    if (isFiltersVisible) {
        filtersRow.style.display = 'none';
        filterButton.textContent = '⛉';

        filterName.textContent = '';
        filterSteamId.textContent = '';
        filterUserDescription.textContent = '';
        filterHighlightDate.textContent = '';
    } else {
        filtersRow.style.display = 'table-row'; // or 'block' if it's not a table row
        filterButton.textContent = '⛊';
    }
    isFiltersVisible = !isFiltersVisible; // Toggle the variable after each click
}
toggleRowVisibility();

document.addEventListener('DOMContentLoaded', function () {
    var filtersRow = document.getElementById('filtersRow');
    var isFiltersVisible = false;

    // Function to toggle the visibility of the filter inputs
    function showFilterInput(header) {
        isFiltersVisible = !isFiltersVisible;
        filtersRow.style.display = isFiltersVisible ? 'table-row' : 'none';

        // Change the header content based on visibility
        var filterButton = document.getElementById('filterButton');
        filterButton.textContent = isFiltersVisible ? '⛊' : '⛉';
    }

    // Function to filter the table rows based on input values
    function filterTable() {
        // Get the table and its rows
        var table = document.getElementById("highlightsTable");
        var rows = table.getElementsByTagName("tbody")[0].getElementsByTagName("tr");

        // Get filter values and preprocess them
        var nameFilter = document.getElementById("nameFilter").value.toLowerCase().replace(/\s+/g, '');
        var steamIdFilter = document.getElementById("steamIdFilter").value.toLowerCase().replace(/\s+/g, '');
        var userDescriptionFilter = document.getElementById("userDescriptionFilter").value.toLowerCase().replace(/\s+/g, '');
        var highlightDateFilter = document.getElementById("highlightDateFilter").value.toLowerCase().replace(/\s+/g, '');

        var visibleRowCount = 0;

        // Iterate over each row
        for (var i = 0; i < rows.length; i++) {
            var cells = rows[i].getElementsByTagName("td");

            // Get text content of relevant columns and preprocess them
            var nameText = cells[1].textContent.toLowerCase().replace(/\s+/g, '');
            var steamIdText = cells[2].textContent.toLowerCase().replace(/\s+/g, '');
            var userDescriptionText = cells[3].textContent.toLowerCase().replace(/\s+/g, '');
            var highlightDateText = cells[5].textContent.toLowerCase().replace(/\s+/g, '');
            

            // Check if each cell's text contains the respective filter text, ignoring spaces
            var nameMatch = nameText.indexOf(nameFilter) > -1;
            var steamIdMatch = steamIdText.indexOf(steamIdFilter) > -1;
            var userDescriptionMatch = userDescriptionText.indexOf(userDescriptionFilter) > -1;
            var highlightDateMatch = highlightDateText.indexOf(highlightDateFilter) > -1;

            // Determine whether to display the row based on filter matches
            if (nameMatch && steamIdMatch && userDescriptionMatch && highlightDateMatch) {
                rows[i].style.display = "";
                visibleRowCount++;
            } else {
                rows[i].style.display = "none";
            }
        }

        var filterResultCountElement = document.getElementById("filterResultCount");
        if (filterResultCountElement) {
            filterResultCountElement.innerHTML = visibleRowCount.toString() + " results";
        }
    }

    // Make the functions globally accessible
    window.showFilterInput = showFilterInput;
    window.filterTable = filterTable;
});


// Upload video and place in DB
function uploadVideo(highlightId) {
    document.getElementById('videoInput_' + highlightId).click();

    var videoInput = document.getElementById('videoInput_' + highlightId);

    if (videoInput && videoInput.files.length > 0) {
        var videoElement = document.getElementById('clip_' + highlightId);
        if (videoElement) {
            videoElement.src = URL.createObjectURL(videoInput.files[0]);
        }
    }

}


// Toggles multiple input/users to save at once selection
function toggleMultipleInput(checkbox) {
    var isAddingMultiple = checkbox.checked;
    var singleInput = document.getElementById("singleInput");
    var multipleInput = document.getElementById("multipleInput");
    var createGroupCheckbox = document.getElementById("createGroup");

    if (isAddingMultiple) {
        // Clear single input values
        document.getElementsByName("SteamID")[0].value = "";
        document.getElementsByName("UserDescription")[0].value = "";
        document.getElementsByName("HighlightDate")[0].value = "";

        singleInput.style.display = "none";
        Array.from(singleInput.querySelectorAll("input")).forEach(input => input.removeAttribute("required"));

        multipleInput.style.display = "flex";
        Array.from(multipleInput.querySelectorAll("input")).forEach(input => input.setAttribute("required", "true"));

        // Make the "Create as a Group" checkbox optional
        createGroupCheckbox.removeAttribute("required");
    } else {
        document.getElementsByName("SteamIDs")[0].value = "";
        document.getElementsByName("UserDescriptionMultiple")[0].value = "";
        document.getElementsByName("HighlightDateMultiple")[0].value = "";

        singleInput.style.display = "flex";
        Array.from(singleInput.querySelectorAll("input")).forEach(input => input.setAttribute("required", "true"));

        multipleInput.style.display = "none";
        Array.from(multipleInput.querySelectorAll("input")).forEach(input => input.removeAttribute("required"));

        // Restore the "Create as a Group" checkbox's required attribute if it's checked
        if (createGroupCheckbox.checked) {
            createGroupCheckbox.setAttribute("required", "true");
        }
    }

    adjustContentHeight();
    adjustTableHeight();
}


function adjustContentHeight() {
    var content = document.getElementById("content");
    var footer = document.getElementById("footer");
    var footerHeight = footer.offsetHeight;
    content.style.maxHeight = `calc(100vh - ${footerHeight}px)`;
}

function adjustTableHeight() {
    var table = document.querySelector(".table-responsive");
    var content = document.getElementById("content");
    var footer = document.getElementById("footer");
    var footerHeight = footer.offsetHeight;
    var contentHeight = content.offsetHeight;
    table.style.maxHeight = `90%`;
}

document.addEventListener("DOMContentLoaded", function () {
    adjustContentHeight();
    adjustTableHeight();
});

window.addEventListener("resize", function () {
    adjustContentHeight();
    adjustTableHeight();
});

var observer = new MutationObserver(function () {
    adjustContentHeight();
    adjustTableHeight();
});

observer.observe(document.getElementById("footer"), { attributes: true, childList: true, subtree: true });

var highlightArray = []; // Define array

function updateModel(newValue, highlightId) {
    // Find the object in memory
    var highlight = highlightArray.find(g => g.ID === highlightId);
    if (highlight) {
        // Update the UserDescription property
        highlight.UserDescription = newValue;
    }
}

// AJAX to save your changes made to the Row selected.
function saveChanges(highlightId) {
    var steamNameElement = document.getElementById('steamName_' + highlightId);
    var steamName = steamNameElement ? steamNameElement.value : '';
    var userDescription = document.getElementById('userDescription_' + highlightId).value;
    var highlightDate = document.getElementById('highlightDate_' + highlightId).value;
    var profilePictureInput = document.getElementById('profilePictureInput_' + highlightId);
    var videoInput = document.getElementById('videoInput_' + highlightId);

    var formData = new FormData();
    formData.append('Id', highlightId);
    formData.append('SteamName', steamName);
    formData.append('UserDescription', userDescription);
    formData.append('HighlightDate', highlightDate);

    if (profilePictureInput && profilePictureInput.files.length > 0) {
        formData.append('ProfilePicture', profilePictureInput.files[0]); // Append the image file
    }

    if (videoInput && videoInput.files.length > 0) {
        formData.append('VideoFile', videoInput.files[0]); // Append the video file
    }

    fetch('/Highlights/EditHighlight/' + highlightId, {
        method: 'POST',
        body: formData
    }).then(response => {
        if (response.ok) {
            // Update the profile picture image
            var profilePictureElement = document.getElementById('profilePicture_' + highlightId);
            if (profilePictureElement) {
                profilePictureElement.src = URL.createObjectURL(profilePictureInput.files[0]);
            }

            // Hide the preview and reset the file input
            discardChanges(highlightId);

            // Update the clip preview if a new video was uploaded
            if (videoInput && videoInput.files.length > 0) {
                var videoElement = document.getElementById('clip_' + highlightId);
                if (videoElement) {
                    videoElement.src = URL.createObjectURL(videoInput.files[0]);
                }
            }

        } else {

        }
    }).catch(error => {
        // not handling error
    });
}

function submitForm(highlightId) {
    document.getElementById('editForm_' + highlightId).submit();
}

function updateFormData(highlightId) {
    // Update the hidden input fields with the values of the edited fields
    document.getElementById('steamNameHidden_' + highlightId).value = document.getElementById('steamName_' + highlightId).value;
    document.getElementById('userDescriptionHidden_' + highlightId).value = document.getElementById('userDescription_' + highlightId).value;
    document.getElementById('highlightDateHidden_' + highlightId).value = document.getElementById('highlightDate_' + highlightId).value;

    // Submit the form
    document.getElementById('editForm_' + highlightId).submit();
}

function discardChanges(highlightId) {
    var input = document.getElementById('profilePictureInput_' + highlightId);
    var preview = document.getElementById('preview_' + highlightId);
    var saveButton = document.getElementById('saveButton_' + highlightId);
    var profilePictureInputBreakLineFirst = document.getElementById('profilePictureInputBreakLineFirst_' + highlightId);
    var profilePictureInputBreakLineSecond = document.getElementById('profilePictureInputBreakLineSecond_' + highlightId);
    var discardButton = document.getElementById('discardButton_' + highlightId);

    input.value = ''; // Clear the file input
    preview.src = ""; // Clear the preview
    preview.style.display = 'none'; // Hide the preview
    saveButton.style.display = 'none'; // Hide the save button
    profilePictureInputBreakLineFirst.style.display = 'none'; // Hide the discard button
    profilePictureInputBreakLineSecond.style.display = 'none'; // Hide the discard button
    discardButton.style.display = 'none'; // Hide the discard button
}


// Setup all required elements with BootStrap JS CSS Datepicker.
$(function () {
    $("#highlightDate").datepicker({
        format: 'mm-dd-yyyy', // Date format
        autoclose: true, // Close the datepicker when a date is selected
        todayHighlight: true // Highlight today's date
    });
});

$(function () {
    $("#highlightDateMultiple").datepicker({
        format: 'mm-dd-yyyy', // Date format
        autoclose: true, // Close the datepicker when a date is selected
        todayHighlight: true // Highlight today's date
    });
});

$(function () {
    $("#highlightDateFilter").datepicker({
        format: 'mm-dd-yyyy', // Date format
        autoclose: true, // Close the datepicker when a date is selected
        todayHighlight: true // Highlight today's date
    });
});


// Script for setting up a preview but not yet saved image upload below the original upload.
function previewImage(input, highlightId) {
    var preview = document.getElementById('preview_' + highlightId);
    var saveButton = document.getElementById('saveButton_' + highlightId);
    var discardButton = document.getElementById('discardButton_' + highlightId);
    var profilePictureInputBreakLineFirst = document.getElementById('profilePictureInputBreakLineFirst_' + highlightId);
    var profilePictureInputBreakLineSecond = document.getElementById('profilePictureInputBreakLineSecond_' + highlightId);


    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            preview.src = e.target.result;
            preview.style.display = 'block';
            saveButton.style.display = 'inline';
            discardButton.style.display = 'inline';
            profilePictureInputBreakLineFirst.style.display = 'inline-block'; // Show the BRs
            profilePictureInputBreakLineSecond.style.display = 'inline-block'; // Show the BRs
        };
        reader.readAsDataURL(input.files[0]);
    } else {
        preview.src = "";
        preview.style.display = 'none';
        saveButton.style.display = 'none';
        discardButton.style.display = 'none';
        profilePictureInputBreakLineFirst.style.display = 'none'; // Hide the BRs
        profilePictureInputBreakLineSecond.style.display = 'none'; // Hide the BRs
    }
}
