function Reading(options) {
    var self = this;
    self.hasChanged = false;
    self.modal = $('#popup');
    self.currentElement = null;
    self.options = options;
    self.wasPlaying = false;
    self.jplayer = $('#jquery_jplayer_1');

    self.setOccurencesText = function (occurs) {
        switch (occurs) {
            case 1:
                $('#dOccurs').text('once');
                break;
            case 2:
                $('#dOccurs').text('twice');
                break;
            default:
                $('#dOccurs').text(occurs + ' times');
                break;
        }
    };

    self.setFrequencyText = function (freq) {
        $('#dFrequency').text(freq);
    };

    self._getWordFromSpan = function (element) {
        if (element[0].childNodes[0].nodeType == 3) {
            return element[0].childNodes[0].nodeValue;
        } else {
            return element[0].childNodes[0].innerText;
        }
    };

    self.setFocus = function (element) {
        if (element.any()) {
            element.focus();
        }
    };

    self.updateCurrentSelected = function (element) {
        if (element.any()) {
            var current = self.getCurrentSelected();
            current.removeClass('__current');
            element.addClass('__current');
        }

        $(document).trigger('postUpdateCurrentSelected');
    };

    self.getCurrentSelected = function () {
        return $('.__current');
    };

    self.getCurrentWordAsText = function () {
        if (self.currentElement.any()) {
            return self._getWordFromSpan(self.currentElement);
        }

        return '';
    };

    self.getCurrentSentence = function () {
        if (!self.currentElement.any()) {
            return '';
        }

        var sentence = '';
        var children = self.currentElement.parent('p.__sentence').children('span');

        for (var i = 0; i < children.length; i++) {
            sentence += $(children[i]).text();
        }

        return sentence;
    };

    self._removeChanged = function () {
        $('#dSentence').removeClass('changed');
        $('#dBase').removeClass('changed');
        $('#dDefinition').removeClass('changed');
    };

    self.copyToClipboard = function (toCopy) {
        $(document).trigger('preCopyToClipboard');
        self.sendChatMessage('modal', toCopy);
        $(document).trigger('postCopyToClipboard');
    };

    self.updateModal = function () {
        $(document).trigger('preUpdateModal');

        var text = self.getCurrentWordAsText();

        self.setDMessage('');
        self._removeChanged();
        self.setOccurencesText(self.getOccurrences());
        self.setFrequencyText(self.getFrequency());

        self.copyToClipboard(text);

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
        }).fail(function (data) {
            if (data.status == 404) {
                $('[name=State][value="unknown"]').prop('checked', 'true');
                $('#dPhrase').html(text);
                $('#dBase').val('');
                $('#dDefinition').val('');
                $('#dSentence').val(self.getCurrentSentence());
                $('#dSentence').addClass('changed');
                $('#dMessage').html('New word, defaulting to unknown');
            } else {
                $('#dPhrase').html(text);
                $('#dMessage').html('failed to lookup word');
            }
        }).always(function (data) {
            $(document).trigger('postUpdateModal');
        });
    };

    self._selectText = function () {
        setTimeout(function () {
            var range = document.createRange();
            var selection = window.getSelection();
            selection.removeAllRanges();
            range.selectNodeContents($('#dPhrase')[0]);
            selection.addRange(range);
        }, 125);
    };

    self.getDState = function () {
        return $('input[name="State"]:checked').val();
    };

    self.getDBase = function () {
        return $('#dBase').val();
    };

    self.getDSentence = function () {
        return $('#dSentence').val();
    };

    self.getDDefinition = function () {
        return $('#dDefinition').val();
    };

    self.getLanguageId = function () {
        return self.options.languageId;
    };

    self.getItemId = function () {
        return self.options.itemId;
    };

    self.setDState = function (state) {
        $('[name=State][value=' + state + ']').prop('checked', 'true');
        self.changed();
    };

    self.setDBase = function (val) {
        $('#dBase').val(val);
        self.changed();
    };

    self.setDSentence = function (val) {
        $('#dSentence').val(val);
        self.changed();
    };

    self.setDDefinition = function (val) {
        $('#dDefinition').val(val);
        self.changed();
    };

    self.setDMessage = function (val) {
        $('#dMessage').html(val);
        self.changed();
    };

    self.save = function () {
        $(document).trigger('preSave');

        var phrase = self.getCurrentWordAsText();
        var state = self.getDState();

        $.ajax({
            url: self.options.url + "/api/terms/",
            type: 'POST',
            data: {
                phrase: phrase,
                basePhrase: self.getDBase(),
                sentence: self.getDSentence(),
                definition: self.getDDefinition(),
                languageId: self.getLanguageId(),
                itemId: self.getItemId(),
                state: state,
            }
        }).done(function (data, status, xhr) {
            if (xhr.status == 200) {
                self.setDMessage('Term updated');
            } else if (xhr.status == 201) {
                self.setDMessage('New term saved');
            } else {
                self.setDMessage('Saved');
            }

            self._removeChanged();
            var lower = phrase.toLowerCase();
            $('.__' + lower).removeClass('__notseen __known __ignored __unknown').addClass('__' + state.toLowerCase());

            var tempDef = self.getDBase().length > 0 ? self.getDBase() + "<br/>" : '';
            if (self.getDDefinition().length > 0) tempDef += self.getDDefinition().replace(/\n/g, '<br />');

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
            self.setDMessage('Save failed');
        }).always(function (data) {
            $(document).trigger('postSave');
        });
    };

    self.reset = function () {
        $(document).trigger('preReset');

        var phrase = self.getCurrentWordAsText();

        $.ajax({
            url: self.options.url + "/api/terms/",
            type: 'DELETE',
            data: {
                phrase: phrase,
                basePhrase: self.setDBase(''),
                sentence: self.setDSentence(''),
                definition: self.setDDefinition(''),
                languageId: self.getLanguageId(),
                itemId: self.getItemId(),
                state: 'NotSeen',
            }
        }).done(function (data, status, xhr) {
            if (xhr.status == 200) {
                self.setDMessage('Term reset, use save to keep data.');
            } else {
                self.setDMessage('Term reset');
            }

            var lower = phrase.toLowerCase();
            $('.__' + lower).removeClass('__notseen __known __ignored __unknown __kd __id __ud').addClass('__notseen');
            $('.__' + lower).each(function (index) {
                $(this).html(phrase);
            });

        }).fail(function (data) {
            self.setDMessage('Reset failed');
        }).always(function (data) {
            $(document).trigger('postReset');
        });
    };

    self.changed = function () {
        self.hasChanged = true;
    };

    self.getHasChanged = function () {
        return self.hasChanged;
    };

    self.setWasPlaying = function () {
        self.wasPlaying = self.jplayer.data() == null ? false : !self.jplayer.data().jPlayer.status.paused;
    };

    self.getWasPlaying = function () {
        return self.wasPlaying;
    };

    self.copy = function () {
        self.setDBase(self.getCurrentWordAsText());
        self.changed();

        $(document).trigger('postWordCopy');
    };

    self.refresh = function () {
        self.setDSentence(self.getCurrentSentence());
        $('#dSentence').addClass('changed');
        self.changed();

        $(document).trigger('postSentenceRefreshed');
    };

    self.getFrequency = function () {
        if (self.currentElement.any()) {
            return self.currentElement.data('frequency');
        }

        return 0;
    };

    self.getOccurrences = function () {
        if (self.currentElement.any()) {
            return self.currentElement.data('occurrences');
        }

        return 0;
    };

    self.displayModal = function () {
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

    self.addMatching = function (element) {
        if (element.any()) {
            element.addClass('__matching');
        }

        $(document).trigger('postAddMatching');
    };

    self.removeMatching = function () {
        $('.__matching').removeClass('__matching');

        $(document).trigger('postRemoveMatching');
    };

    self.showModal = function (element) {
        $(document).trigger('preShowModal');

        if (self.modal.is(':visible') && self.hasChanged) {
            return;
        }

        self.currentElement = element;
        self.removeMatching();
        self.updateCurrentSelected(element);

        var cls = self.currentElement.data('lower');
        self.addMatching($('.__' + cls));

        self.setWasPlaying();

        if (self.getWasPlaying()) {
            self.jplayer.jPlayer('pause');
        }

        self.updateModal();
        self.displayModal();

        $(document).trigger('postShowModal');
    };

    self.closeModal = function () {
        $(document).trigger('preCloseModal');

        self.hasChanged = false;
        self.removeMatching();
        self.modal.hide();

        if (self.getWasPlaying()) {
            self.jplayer.jPlayer("play", self.jplayer.data().jPlayer.status.currentTime - 1);
            self.jplayer.jPlayer("play");
        }

        $(document).trigger('postCloseModal');
    };

    self.getItemType = function () {
        return $('#reading').data('itemtype');
    };

    self.getPlayer = function () {
        return self.jplayer;
    };

    self.sendChatMessage = function (source, message) {
        self.options.chat.server.send(source, message);
    };

    if (self.getItemType() == 'video') {
        self.jplayer.bind($.jPlayer.event.timeupdate, function (event) {
            self.sendChatMessage('video', event.jPlayer.status.currentTime);
        });
    }

    $(document).trigger('pluginReady');
}