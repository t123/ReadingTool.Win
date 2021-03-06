﻿$(function () {
    jQuery.ajaxSettings.traditional = true;
    jQuery.support.cors = true;
    window.reading = undefined;

    var webApiEndPoint = $('#reading').data('webapi');
    var signalREndPoint = $('#reading').data('signalr') + "/signalr";

    $.connection.hub.url = signalREndPoint;
    var chat = $.connection.mainHub;

    window.hubReady = $.connection.hub.start();

    var lastL1 = -2, lastL2 = -2;

    chat.client.addMessage = function (name, message) {
        if (name == "srtl1") {
            if (message == lastL1) {
                return;
            }

            if (message == -1) {
                $('#l1Main').html('');
            } else {
                $('#l1Main').html($('#l1_' + message).html());
            }

            lastL1 = message;

        } else if (name == "srtl2") {
            if (message == lastL2) {
                return;
            }

            if (message == -1) {
                $('#l2Main').html('');
            } else {
                $('#l2Main').html($('#l2_' + message).html());
            }

            lastL2 = message;
        } else if (name == "markremainingasknown") {
            if (!confirm('Are you sure you want to mark remaining words as known?')) {
                return false;
            }

            reading.markRemainingAsKnown();
        } else if (name == "read") {
            window.reading.changeRead(message, 'read');
        } else if (name == "listen") {
            window.reading.changeRead(message, 'listen');
        }
    };

    $('body').tooltip({
        selector: '[rel=tooltip]',
        html: true,
        animation: false
    });

    window.hubReady.done(function () {
        if ($('#reading').data('mediauri') != '') {
            var mediaUri = webApiEndPoint + '/api/v1/resource/media/' + $('#reading').data('itemid');

            if ($('#reading').data('itemtype') == 'text') {
                $("#jquery_jplayer_1").jPlayer({
                    ready: function () {
                        $(this).jPlayer("setMedia", {
                            mp3: mediaUri
                        });
                    },
                    swfPath: webApiEndPoint + "/api/v1/resource/local/Jplayer.swf",
                    supplied: "mp3",
                    errorAlerts: true
                });
            } else {
                $("#jquery_jplayer_1").jPlayer({
                    ready: function () {
                        $(this).jPlayer("setMedia", {
                            m4v: mediaUri
                        });
                    },
                    swfPath: webApiEndPoint + "/api/v1/resource/local/Jplayer.swf",
                    supplied: "m4v",
                    errorAlerts: true,
                    size: {
                        width: "720px",
                        height: "405px"
                    }
                });
            }
        } else {
            $('#jp_container_1').hide();
        }

        var reading = new Reading({
            url: webApiEndPoint,
            languageId: $('#reading').data('languageid'),
            itemId: $('#reading').data('itemid'),
            chat: chat
        });

        window.reading = reading;
        $(document).trigger('pluginReady');

        $(document).on('keydown', function (e) {
            var code = (e.keyCode ? e.keyCode : e.which);

            if ($('#popup').is(':visible')) {
                if (e.ctrlKey) {
                    switch (code) {
                        case 13: //Enter
                            reading.save();
                            reading.closeModal();
                            e.preventDefault();
                            break;

                        case 82: //R
                            reading.reset();
                            e.preventDefault();
                            break;

                        case 49: //1
                            reading.setDState('known');
                            e.preventDefault();
                            break;

                        case 50: //2
                            reading.setDState('unknown');
                            e.preventDefault();
                            break;

                        case 51: //3
                            reading.setDState('ignored');
                            e.preventDefault();
                            break;

                        case 52: //4
                            reading.setDState('notseen');
                            e.preventDefault();
                            break;

                        case 81: //q
                        case 83: //s
                            reading.setFocus($('#dSentence'));
                            e.preventDefault();
                            break;

                        case 65: //a
                        case 66: //b
                            reading.setFocus($('#dBase'));
                            e.preventDefault();
                            break;

                        case 90: //z
                        case 68: //d
                            reading.setFocus($('#dDefinition'));
                            e.preventDefault();
                            break;
                    }

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
                        var el = reading.getCurrentSelected();

                        if (el.any()) {
                            reading.showModal(el);
                            e.preventDefault();
                            return;
                        }
                        break;

                    case 37: //left
                        var el = $('.__current').prevAll('.__term.__notseen,.__term.__unknown').first('span');

                        if (el.any()) {
                            reading.updateCurrentSelected(el);
                            return;
                        }

                        el = $('.__current').parent().prev('.__sentence').children('.__term.__notseen,.__term.__unknown').last('span');

                        if (el.any()) {
                            reading.updateCurrentSelected(el);
                            return;
                        }
                        break;

                    case 39: //right
                        var el = $('.__current').nextAll('.__term.__notseen,.__term.__unknown').first('span');

                        if (el.any()) {
                            reading.updateCurrentSelected(el);
                            return;
                        }

                        el = $('.__current').parent().next('.__sentence').children('.__term.__notseen,.__term.__unknown').first('span');

                        if (el.any()) {
                            reading.updateCurrentSelected(el);
                            return;
                        }
                        break;
                }
            }
        });

        $('input[type="text"], input[type="radio"], textarea').change(function (e) {
            $(document).trigger('preDataChanged', e, e);
            reading.changed($(e.target));
            $(document).trigger('postDataChanged', e, e);
        });

        jQuery.fn['any'] = function () {
            return (this.length > 0);
        };

        $('#reading').on('click', 'span.__term', function (e) {
            if (e.ctrlKey) {
                reading.markTemp($(this));
            } else {
                reading.showModal($(this));
            }
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

        $('#btnMark').click(function () {
            if (!confirm('Are you sure you want to mark remaining words as known?')) {
                return false;
            }

            reading.markRemainingAsKnown();

            return false;
        });
    });
});

