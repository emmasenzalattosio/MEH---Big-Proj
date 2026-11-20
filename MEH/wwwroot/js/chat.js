// LIVE-CHAT
// 1. Button unten rechts öffnet ein Fenster mit allen Mitarbeitern
// 2. Klick auf einen Mitarbeiter -> Verlauf laden (/Chat/Verlauf/ID) und schreiben
// 3. Senden + Empfangen läuft live über SignalR (/chatHub -> Hubs/ChatHub.cs)
// 4. Neue Nachricht, während der Chat nicht offen ist -> rote Zahl am Button + kurzer Hinweis
(function () {
    const root = document.getElementById('sdChat');
    if (!root || typeof signalR === 'undefined') return;

    const meineId = parseInt(root.dataset.meineId, 10);

    const el = {
        button: document.getElementById('sdChatButton'),
        badge: document.getElementById('sdChatBadge'),
        fenster: document.getElementById('sdChatFenster'),
        titel: document.getElementById('sdChatTitel'),
        zurueck: document.getElementById('sdChatZurueck'),
        schliessen: document.getElementById('sdChatSchliessen'),
        kontakte: document.getElementById('sdChatKontakte'),
        gespraech: document.getElementById('sdChatGespraech'),
        verlauf: document.getElementById('sdChatVerlauf'),
        form: document.getElementById('sdChatForm'),
        text: document.getElementById('sdChatText'),
        toast: document.getElementById('sdChatToast')
    };

    let aktiverKontakt = null; // { id, name } oder null, wenn die Liste angezeigt wird
    let ungelesenGesamt = 0;
    let toastTimer = null;

    // ---------- Verbindung zum Server ----------
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/chatHub')
        .withAutomaticReconnect()
        .build();

    connection.on('NeueNachricht', function (n) {
        const vonMir = n.vonId === meineId;
        const anderePerson = vonMir ? n.anId : n.vonId;

        // Gespräch mit dieser Person ist gerade offen -> direkt anzeigen
        if (istFensterOffen() && aktiverKontakt && aktiverKontakt.id === anderePerson) {
            zeigeNachricht(n);
            if (!vonMir) connection.invoke('MarkiereGelesen', n.vonId).catch(console.error);
            return;
        }

        // Sonst: Benachrichtigung
        if (!vonMir) {
            setzeBadge(ungelesenGesamt + 1);
            zeigeToast(n.vonName, n.text);
            if (istFensterOffen() && !aktiverKontakt) ladeKontakte(); // Zahlen in der Liste aktualisieren
        }
    });

    connection.start()
        .then(ladeUngelesen)
        .catch(function (err) { console.error('Chat-Verbindung fehlgeschlagen:', err); });

    connection.onreconnected(ladeUngelesen);

    // ---------- Daten vom Server ----------
    function ladeUngelesen() {
        return fetch('/Chat/Ungelesen')
            .then(r => r.json())
            .then(setzeBadge);
    }

    function ladeKontakte() {
        return fetch('/Chat/Kontakte')
            .then(r => r.json())
            .then(function (liste) {
                el.kontakte.innerHTML = '';
                if (liste.length === 0) {
                    const li = document.createElement('li');
                    li.className = 'sd-chat-leer';
                    li.textContent = 'Noch keine anderen Mitarbeiter vorhanden.';
                    el.kontakte.appendChild(li);
                }
                let summe = 0;
                liste.forEach(function (k) {
                    summe += k.ungelesen;
                    const li = document.createElement('li');
                    const btn = document.createElement('button');
                    btn.type = 'button';
                    btn.className = 'sd-chat-kontakt';

                    const avatar = document.createElement('span');
                    avatar.className = 'sd-chat-avatar';
                    avatar.textContent = initialen(k.name);

                    const info = document.createElement('span');
                    info.className = 'sd-chat-kontakt-info';
                    const name = document.createElement('strong');
                    name.textContent = k.name;
                    const bereich = document.createElement('small');
                    bereich.textContent = k.fachbereich;
                    info.append(name, bereich);

                    btn.append(avatar, info);

                    if (k.ungelesen > 0) {
                        const zahl = document.createElement('span');
                        zahl.className = 'sd-chat-kontakt-zahl';
                        zahl.textContent = k.ungelesen;
                        btn.appendChild(zahl);
                    }

                    btn.addEventListener('click', function () { oeffneGespraech(k); });
                    li.appendChild(btn);
                    el.kontakte.appendChild(li);
                });
                setzeBadge(summe);
            });
    }

    function oeffneGespraech(kontakt) {
        aktiverKontakt = { id: kontakt.id, name: kontakt.name };
        el.titel.textContent = kontakt.name;
        el.kontakte.hidden = true;
        el.gespraech.hidden = false;
        el.zurueck.hidden = false;
        el.verlauf.innerHTML = '';

        fetch('/Chat/Verlauf/' + kontakt.id)
            .then(r => r.json())
            .then(function (nachrichten) {
                if (nachrichten.length === 0) {
                    const leer = document.createElement('div');
                    leer.className = 'sd-chat-leer';
                    leer.textContent = 'Noch keine Nachrichten. Schreib die erste!';
                    el.verlauf.appendChild(leer);
                }
                nachrichten.forEach(zeigeNachricht);
                return ladeUngelesen(); // Verlauf hat die Nachrichten als gelesen markiert
            });

        el.text.focus();
    }

    function zeigeListe() {
        aktiverKontakt = null;
        el.titel.textContent = 'Chat';
        el.kontakte.hidden = false;
        el.gespraech.hidden = true;
        el.zurueck.hidden = true;
        ladeKontakte();
    }

    // ---------- Anzeige ----------
    function zeigeNachricht(n) {
        const leer = el.verlauf.querySelector('.sd-chat-leer');
        if (leer) leer.remove();

        const blase = document.createElement('div');
        blase.className = 'sd-chat-blase ' + (n.vonId === meineId ? 'sd-chat-ich' : 'sd-chat-du');

        const text = document.createElement('div');
        text.textContent = n.text; // textContent statt innerHTML -> kein HTML/Script aus Nachrichten
        const zeit = document.createElement('small');
        zeit.textContent = n.gesendetAm;

        blase.append(text, zeit);
        el.verlauf.appendChild(blase);
        el.verlauf.scrollTop = el.verlauf.scrollHeight;
    }

    function setzeBadge(anzahl) {
        ungelesenGesamt = Math.max(0, anzahl || 0);
        el.badge.textContent = ungelesenGesamt > 99 ? '99+' : ungelesenGesamt;
        el.badge.hidden = ungelesenGesamt === 0;
    }

    function zeigeToast(name, text) {
        el.toast.innerHTML = '';
        const titel = document.createElement('strong');
        titel.textContent = name;
        const vorschau = document.createElement('div');
        vorschau.textContent = text.length > 60 ? text.slice(0, 60) + ' …' : text;
        el.toast.append(titel, vorschau);
        el.toast.hidden = false;
        clearTimeout(toastTimer);
        toastTimer = setTimeout(function () { el.toast.hidden = true; }, 4000);
    }

    function istFensterOffen() {
        return !el.fenster.hidden;
    }

    function initialen(name) {
        return name.split(' ').filter(Boolean).map(t => t[0]).join('').slice(0, 2).toUpperCase();
    }

    // ---------- Klicks ----------
    el.button.addEventListener('click', function () {
        if (istFensterOffen()) {
            el.fenster.hidden = true;
        } else {
            el.fenster.hidden = false;
            el.toast.hidden = true;
            if (aktiverKontakt) {
                oeffneGespraech(aktiverKontakt);
            } else {
                zeigeListe();
            }
        }
    });

    el.toast.addEventListener('click', function () {
        el.toast.hidden = true;
        el.fenster.hidden = false;
        zeigeListe();
    });

    el.schliessen.addEventListener('click', function () { el.fenster.hidden = true; });
    el.zurueck.addEventListener('click', zeigeListe);

    el.form.addEventListener('submit', function (e) {
        e.preventDefault();
        const text = el.text.value.trim();
        if (!text || !aktiverKontakt) return;

        connection.invoke('SendeNachricht', aktiverKontakt.id, text)
            .then(function () { el.text.value = ''; })
            .catch(function (err) {
                console.error(err);
                zeigeToast('Fehler', 'Nachricht konnte nicht gesendet werden.');
            });
    });
})();
