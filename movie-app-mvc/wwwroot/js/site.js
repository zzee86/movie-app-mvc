function showSignInPopup() {
    alert("You must be signed in to perform this action.");
}

function showVideo(movieKey) {
    const overlayVideo = document.getElementById('overlay-video');
    const closeVideoButton = document.getElementById('close-button');
    const videoIframe = document.getElementById('video-iframe');

    overlayVideo.style.display = 'flex';
    videoIframe.src = `https://www.youtube.com/embed/${movieKey}?autoplay=1`;
    closeVideoButton.style.display = 'block';
}

function closeVideo() {
    const overlayVideo = document.getElementById('overlay-video');
    const videoIframe = document.getElementById('video-iframe');
    const closeVideoButton = document.getElementById('close-button');

    overlayVideo.style.display = 'none';
    videoIframe.src = '';
    closeVideoButton.style.display = 'none';
}
