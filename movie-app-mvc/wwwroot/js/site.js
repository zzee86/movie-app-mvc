$(document).ready(function () {
    // Event handler for remove movie form submission
    $("form.remove-movie").on("submit", function (event) {
        event.preventDefault();

        var form = $(this);

        $.post(form.attr("action"), form.serialize())
            .done(function (response) {
                if (response.success) {
                    // Reload the current page
                    location.reload();
                } else {
                    // Handle failure or display an error message
                    console.log("Failed to remove the movie.");
                }
            })
            .fail(function () {
                // Handle failure or display an error message
                console.log("Failed to remove the movie.");
            });
    });
});
