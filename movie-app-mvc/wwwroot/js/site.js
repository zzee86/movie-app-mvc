function showSignInPopup() {
    alert("You must be signed in to perform this action.");
}




function handleOverlayVideo() {
    // Get the elements
    const showVideoButton = document.getElementById('show-video-button');
    const overlayVideo = document.getElementById('overlay-video');
    const closeVideoButton = document.getElementById('close-button');
    const videoIframe = document.getElementById('video-iframe');

    // Add click event listener to the show video button
    showVideoButton.addEventListener('click', function () {
        overlayVideo.style.display = 'flex';
        videoIframe.src = 'https://www.youtube.com/embed/6JnN1DmbqoU?autoplay=1';
        closeVideoButton.style.display = 'block';
    });

    // Add click event listener to the close video button
    closeVideoButton.addEventListener('click', function () {
        overlayVideo.style.display = 'none';
        videoIframe.src = '';
        closeVideoButton.style.display = 'none';
    });
}

// Call the method to handle the overlay video functionality
handleOverlayVideo();