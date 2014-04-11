var gOldOnError = window.onerror;

window.onerror = function myErrorHandler(errorMsg, url, lineNumber) {
    if (gOldOnError)
    // Call previous handler.
        return gOldOnError(errorMsg, url, lineNumber);

    // Just let default handler run.
    return false;
};

$(function () {
    jQuery.support.cors = true;

    $.connection.hub.url = "http://localhost:8888/signalr";
    var chat = $.connection.mainHub;

    $.connection.hub.start().done(function () {
    });

    chat.client.addMessage = function (name, message) {
        if (name == "srtl1") {
            if (message == -1) {
                $('#l1Main').html('');
            } else {
                $('#l1Main').html($('#l1_' + message).html());
            }
        } else if (name == "srtl2") {
            if (message == -1) {
                $('#l2Main').html('');
            } else {
                $('#l2Main').html($('#l2_' + message).html());
            }
        } else if (name == "mpr") {
            reading.setWasPlaying(message);
        }
    };

    $('body').tooltip({
        selector: '[rel=tooltip]',
        html: true,
        animation: false
    });

    if ($('#reading').data('mediauri') != '') {
        var mediaUri = 'http://localhost:9000/api/media/' + $('#reading').data('itemid');
        $("#jquery_jplayer_1").jPlayer({
            ready: function() {
                $(this).jPlayer("setMedia", {
                    mp3: mediaUri
                });
            },
            swfPath: "http://localhost:9000/api/local/Jplayer.swf",
            supplied: "mp3",
            errorAlerts: true
        });
    }

    var reading = new Reading({
        url: 'http://localhost:9000',
        languageId: $('#reading').data('languageid'),
        itemId: $('#reading').data('itemid'),
        chat: chat
    });

    $(document).on('keydown', function (e) {
        var code = (e.keyCode ? e.keyCode : e.which);

        if ($('#popup').is(':visible')) {
            if (e.ctrlKey) {
                switch (code) {
                    case 13: //Enter
                        reading.save();
                        reading.closeModal();
                        break;

                    case 82: //R
                        reading.reset();
                        break;

                    case 49: //1
                        reading.changeState('known');
                        break;

                    case 50: //2
                        reading.changeState('unknown');
                        break;

                    case 51: //3
                        reading.changeState('ignored');
                        break;

                    case 52: //4
                        reading.changeState('notseen');
                        break;

                    case 81: //q
                    case 83: //s
                        $('#dSentence').focus();
                        break;

                    case 65: //a
                    case 66: //b
                        $('#dBase').focus();
                        break;

                    case 90: //z
                    case 68: //d
                        $('#dDefinition').focus();
                        break;
                }

                e.preventDefault();
                return;
            }

            switch (code) {
                case 27: //escape
                    reading.closeModal();
                    break;

                case 37: //left
                    if (!reading.getHasChanged()) {
                        var el = $('.__current').prevAll('.__term.__notseen,.__term.__unknown').first('span');

                        if (el.any()) {
                            reading.showModal(el);
                            return;
                        }

                        el = $('.__current').parent().prev('.__sentence').children('.__term.__notseen,.__term.__unknown').last('span');

                        if (el.any()) {
                            reading.showModal(el);
                            return;
                        }
                    }
                    break;

                case 39: //right
                    if (!reading.getHasChanged()) {
                        var el = $('.__current').nextAll('.__term.__notseen,.__term.__unknown').first('span');

                        if (el.any()) {
                            reading.showModal(el);
                            return;
                        }

                        el = $('.__current').parent().next('.__sentence').children('.__term.__notseen,.__term.__unknown').first('span');

                        if (el.any()) {
                            reading.showModal(el);
                            return;
                        }
                    }
                    break;
            }
        } else {
            switch (code) {
                case 32:
                    var el = $('.__current');
                    if (el.any()) {
                        reading.showModal(el);
                        e.preventDefault();
                        return;
                    }
                    break;

                case 37: //left
                    var current = $('.__current');
                    var el = $('.__current').prevAll('.__term.__notseen,.__term.__unknown').first('span');

                    if (el.any()) {
                        current.removeClass('__current');
                        el.addClass('__current');
                        return;
                    }

                    el = $('.__current').parent().prev('.__sentence').children('.__term.__notseen,.__term.__unknown').last('span');

                    if (el.any()) {
                        current.removeClass('__current');
                        el.addClass('__current');
                        return;
                    }
                    break;

                case 39: //right
                    var current = $('.__current');
                    var el = $('.__current').nextAll('.__term.__notseen,.__term.__unknown').first('span');

                    if (el.any()) {
                        current.removeClass('__current');
                        el.addClass('__current');
                        return;
                    }

                    el = $('.__current').parent().next('.__sentence').children('.__term.__notseen,.__term.__unknown').first('span');

                    if (el.any()) {
                        current.removeClass('__current');
                        el.addClass('__current');
                        return;
                    }
                    break;
            }
        }
    });

    $('input[type="text"], input[type="radio"], textarea').change(function (e) {
        $(e.target).addClass('changed');
        reading.changed();
    });

    jQuery.fn['any'] = function () {
        return (this.length > 0);
    };

    $('#reading').on('click', 'span.__term', function () {
        reading.showModal($(this));
    });

    $('#dSave').click(function () {
        reading.save();
    });

    $('#dClose').click(function () {
        reading.closeModal();
        return false;
    });

    $('#dReset').click(function () {
        reading.reset();
    });

    $('#dCopy').click(function () {
        reading.copy();
        return false;
    });

    $('#dRefresh').click(function () {
        reading.refresh();
        return false;
    });
});