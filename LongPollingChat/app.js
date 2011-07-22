$(function () {

    var showMessage = function (message) {
        $('#messages').prepend($('<li/>').append(message));
    };

    var getMessages = function () {
        $.ajax({
            type: 'POST',
            url: '/wait',
            data: 'data=none',
            success: function (data) {
                showMessage(data);
            },
            complete: function (q, s) {
                getMessages();
            }
        });
    };

    var sendMessage = function (message) {
        $.ajax({
            type: 'POST',
            url: '/send',
            data: message,
            contentType: "text/plain",
            success: function (data) {
            },
            error: function (d, m, et) {
                alert(m + ": " + et);
            }
        });
    };

    $('#send-message').click(function (e) {
        e.preventDefault();
        var m = $('#message').val();
        sendMessage(m);
    });

    getMessages();

});