function showSignInPopup() {
    alert("You must be signed in to perform this action.");
}

function showVideo(movieKey) {
    const overlayVideo = document.getElementById('overlay-video');
    const videoIframe = document.getElementById('video-iframe');
    const closeVideoButton = document.getElementById('close-button');

    overlayVideo.style.display = 'flex';
    videoIframe.src = `https://www.youtube.com/embed/${movieKey}?autoplay=1`;
    closeVideoButton.style.display = 'block';

    setTimeout(() => {
        overlayVideo.style.opacity = '1';
    }, 10); 
}

function closeVideo() {
    const overlayVideo = document.getElementById('overlay-video');
    const videoIframe = document.getElementById('video-iframe');
    const closeVideoButton = document.getElementById('close-button');

    overlayVideo.style.opacity = '0';
    videoIframe.src = '';
    closeVideoButton.style.display = 'none';

    setTimeout(() => {
        overlayVideo.style.display = 'none';
    }, 300);
}


