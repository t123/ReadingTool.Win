function Reading(options) {
    var self = this;
    self.hasChanged = false;
    self.modal = $('#popup');
    self.currentElement = null;
    self.options = options;
    self.wasPlaying = false;
    self.jplayer = $('#jquery_jplayer_1');

    self._getCurrentSentence = function () {
        var sentence = '';
        var children = self.currentElement.parent('p.__sentence').children('span');

        for (var i = 0; i < children.length; i++) {
            sentence += $(children[i]).text();
        }

        return sentence;
    };

    self._updateOccurencesText = function (occurs) {
        switch (occurs) {
            case 1:
                return 'once';
            case 2:
                return 'twice';
            default:
                return occurs + ' times';
        }
    };

    self._getWordFromSpan = function (element) {
        if (element[0].childNodes[0].nodeType == 3) {
            return element[0].childNodes[0].nodeValue;
        } else {
            return element[0].childNodes[0].innerText;
        }
    };

    self._getCurrentWordFromSpan = function () {
        return self._getWordFromSpan(self.currentElement);
    };

    self._removeChanged = function () {
        $('#dSentence').removeClass('changed');
        $('#dBase').removeClass('changed');
        $('#dDefinition').removeClass('changed');
    };

    self._updateModal = function () {
        var text = self._getCurrentWordFromSpan();

        $('#dMessage').html('');
        self._removeChanged();
        $('#dOccurs').text(self._updateOccurencesText(self.currentElement.data('occurrences')));
        $('#dFrequency').text(self.currentElement.data('frequency'));

        self.options.chat.server.send("modal", text);

        $.ajax({
            url: self.options.url + '/api/terms/',
            type: 'GET',
            data: {
                phrase: text,
                languageId: self.options.languageId
            }
        }).done(function (data) {
            $('[name=State][value=' + data.State + ']').prop('checked', 'true');
            $('#dPhrase').html(data.Phrase);
            $('#dBase').val(data.BasePhrase);
            $('#dSentence').val(data.Sentence);
            $('#dDefinition').val(data.Definition);

            self._selectText();
        }).fail(function (data) {
            if (data.status == 404) {
                $('[name=State][value="unknown"]').prop('checked', 'true');
                $('#dPhrase').html(text);
                $('#dBase').val('');
                $('#dDefinition').val('');
                $('#dSentence').val(self._getCurrentSentence());
                $('#dSentence').addClass('changed');
                $('#dMessage').html('New word, defaulting to unknown');

                self._selectText();
            } else {
                $('#dPhrase').html(text);
                $('#dMessage').html('failed to lookup word');
            }
        });
    };

    self._selectText = function () {
        //setTimeout(function () {
        //    var range = document.createRange();
        //    var selection = window.getSelection();
        //    selection.removeAllRanges();
        //    range.selectNodeContents($('#dPhrase')[0]);
        //    selection.addRange(range);
        //}, 125);
    };

    self.save = function () {
        var phrase = self._getCurrentWordFromSpan();
        var state = $('input[name="State"]:checked').val();

        $.ajax({
            url: self.options.url + "/api/terms/",
            type: 'POST',
            data: {
                phrase: phrase,
                basePhrase: $('#dBase').val(),
                sentence: $('#dSentence').val(),
                definition: $('#dDefinition').val(),
                languageId: self.options.languageId,
                itemId: self.options.itemId,
                state: state,
            }
        }).done(function (data, status, xhr) {
            if (xhr.status == 200) {
                $('#dMessage').html('Term updated');
            } else if (xhr.status == 201) {
                $('#dMessage').html('New term saved');
            } else {
                $('#dMessage').html('Saved');
            }

            self._removeChanged();
            var lower = phrase.toLowerCase();
            $('.__' + lower).removeClass('__notseen __known __ignored __unknown').addClass('__' + state.toLowerCase());

            var tempDef = $('#dBase').val().length > 0 ? $('#dBase').val() + "<br/>" : '';
            if ($('#dDefinition').val().length > 0) tempDef += $('#dDefinition').val().replace(/\n/g, '<br />');

            if (tempDef.length > 0) {
                $('.__' + lower).each(function (index) {
                    $(this).html(
                        (tempDef.length > 0 ? '<a rel="tooltip" title="' + tempDef + '">' : '') + phrase + (tempDef.length > 0 ? '</a>' : '')
                    );

                    var stateLower = state.toLowerCase();

                    if (stateLower == 'known') {
                        $(this).addClass("__kd");
                    } else if (stateLower == 'unknown') {
                        $(this).addClass("__ud");
                    } else if (stateLower == 'ignored') {
                        $(this).addClass("__id");
                    }
                });
            }
        }).fail(function (data) {
            $('#dMessage').html('Saved failed');
        });
    };

    self.reset = function () {
        var phrase = self._getCurrentWordFromSpan();

        $.ajax({
            url: self.options.url + "/api/terms/",
            type: 'DELETE',
            data: {
                phrase: phrase,
                basePhrase: '',
                sentence: '',
                definition: '',
                languageId: self.options.languageId,
                itemId: self.options.itemId,
                state: 'NotSeen',
            }
        }).done(function (data, status, xhr) {
            if (xhr.status == 200) {
                $('#dMessage').html('Term reset, use save to keep data.');
            } else {
                $('#dMessage').html('Reset');
            }

            var lower = phrase.toLowerCase();
            $('.__' + lower).removeClass('__notseen __known __ignored __unknown __kd __id __ud __t').addClass('__notseen');
            $('.__' + lower).each(function (index) {
                $(this).html(phrase);
            });

        }).fail(function (data) {
            $('#dMessage').html('Reset failed');
        });
    };

    self.changeState = function (state) {
        $('[name=State][value=' + state + ']').prop('checked', 'true');
        self.changed();
    };

    self.changed = function () {
        self.hasChanged = true;
    };

    self.getHasChanged = function () {
        return self.hasChanged;
    };

    self.setWasPlaying = function() {
        self.wasPlaying = self.jplayer.data() == null ? false : !self.jplayer.data().jPlayer.status.paused;
    };

    self.getWasPlaying = function () {
        return self.wasPlaying;
    };

    self.copy = function () {
        $('#dBase').val($('#dPhrase').text());
        self.changed();
    };

    self.refresh = function () {
        $('#dSentence').val(self._getCurrentSentence());
        $('#dSentence').addClass('changed');
        self.changed();
    };

    self._displayModal = function () {
        var c = self.currentElement[0].getBoundingClientRect();

        var dh = $(window).height();
        var dw = $(window).width();

        var o = self.currentElement.offset();
        var nt, nl;

        if (c.top + 21 + 220 > dh) {
            nt = o.top - 215;
        } else {
            nt = o.top + 21;
        }

        if (o.left + 510 < dw) {
            nl = o.left;
        } else {
            nl = o.left - 430;
        }

        self.modal.css('display', 'inline-block');
        self.modal.offset({ top: nt, left: nl });
    };

    self.showModal = function (element) {
        if (self.modal.is(':visible') && self.hasChanged) {
            return;
        }

        self.currentElement = element;
        $('.__current').removeClass('__current');
        $('.__matching').removeClass('__matching');
        element.addClass('__current');

        var cls = self.currentElement.data('lower');
        $('.__' + cls).addClass('__matching');

        self.setWasPlaying();

        if (self.getWasPlaying()) {
            self.jplayer.jPlayer('pause');
        }

        self._updateModal();
        self._displayModal();
    };

    self.closeModal = function () {
        self.hasChanged = false;
        $('.__matching').removeClass('__matching');
        self.modal.hide();

        if (self.getWasPlaying()) {
            self.jplayer.jPlayer("play", self.jplayer.data().jPlayer.status.currentTime - 1);
            self.jplayer.jPlayer("play");
        }
    };

    self.jplayer.bind($.jPlayer.event.timeupdate, function (event) {
        self.options.chat.server.send("video", event.jPlayer.status.currentTime);
    });
}