// LIVE-CHAT
// 1. Button unten rechts öffnet ein Fenster mit allen Mitarbeitern
// 2. Klick auf einen Mitarbeiter -> Verlauf laden (/Chat/Verlauf/ID) und schreiben
// 3. Senden über /Chat/Senden + Echtzeit-Push über SignalR (/chatHub) + automatischer Live-Sync
// 4. Neue Nachricht, während der Chat nicht offen ist -> rote Zahl am Button + Toast-Hinweis
(function () {
    const root = document.getElementById('sdChat');
    if (!root) return;

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
    let letzteToastNachrichtId = 0;
    const angezeigteIds = new Set(); // verhindert doppelte Anzeige bei gleichzeitigem Push + Sync

    // ---------- Verbindung zum Server (SignalR Echtzeit) ----------
    let connection = null;
    if (typeof signalR !== 'undefined') {
        connection = new signalR.HubConnectionBuilder()
            .withUrl('/chatHub')
            .withAutomaticReconnect()
            .build();

        connection.on('NeueNachricht', function (n) {
            verarbeiteEingehendeNachricht(n);
        });

        connection.start()
            .then(ladeUngelesen)
            .catch(function (err) {
                console.warn('SignalR Hinweis (Fallback aktiv):', err);
                ladeUngelesen();
            });

        connection.onreconnected(ladeUngelesen);
    } else {
        ladeUngelesen();
    }

    function verarbeiteEingehendeNachricht(n) {
        const vonMir = n.vonId === meineId;
        const anderePerson = vonMir ? n.anId : n.vonId;

        if (!vonMir && n.id) {
            letzteToastNachrichtId = Math.max(letzteToastNachrichtId, n.id);
        }

        // Gespräch mit dieser Person ist gerade offen -> direkt anzeigen
        if (istFensterOffen() && aktiverKontakt && aktiverKontakt.id === anderePerson) {
            zeigeNachricht(n);
            if (!vonMir && connection && connection.state === 'Connected') {
                connection.invoke('MarkiereGelesen', n.vonId).catch(function () {});
            }
            return;
        }

        // Sonst: Benachrichtigung + Badge aktualisieren
        if (!vonMir) {
            setzeBadge(ungelesenGesamt + 1);
            zeigeToast(n.vonName || 'Neue Nachricht', n.text);
            if (istFensterOffen() && !aktiverKontakt) {
                ladeKontakte();
            }
        }
    }

    // ---------- Automatischer Live-Sync (fängt alles ab, falls WebSockets blockiert sind) ----------
    setInterval(function () {
        if (istFensterOffen() && aktiverKontakt) {
            // Aktives Gespräch: neue Nachrichten abholen
            fetch('/Chat/Verlauf/' + aktiverKontakt.id, { cache: 'no-store' })
                .then(function (r) { return r.ok ? r.json() : []; })
                .then(function (nachrichten) {
                    nachrichten.forEach(zeigeNachricht);
                })
                .catch(function () {});
        } else {
            // Chat geschlossen oder Kontaktliste offen: Ungelesene prüfen
            ladeUngelesen();
            if (istFensterOffen() && !aktiverKontakt) {
                ladeKontakte();
            }
        }
    }, 2500);

    // ---------- Daten vom Server ----------
    function ladeUngelesen() {
        return fetch('/Chat/Ungelesen', { cache: 'no-store' })
            .then(function (r) { return r.ok ? r.json() : null; })
            .then(function (daten) {
                if (daten === null) return;
                // Unterstützt sowohl Zahl als auch Objekt { anzahl, neueste }
                if (typeof daten === 'number') {
                    setzeBadge(daten);
                    return;
                }
                const vorher = ungelesenGesamt;
                setzeBadge(daten.anzahl);

                if (daten.neueste && daten.neueste.id > letzteToastNachrichtId) {
                    if (letzteToastNachrichtId > 0 || daten.anzahl > vorher) {
                        zeigeToast(daten.neueste.vonName, daten.neueste.text);
                    }
                    letzteToastNachrichtId = daten.neueste.id;
                }
            })
            .catch(function () {});
    }

    function ladeKontakte() {
        return fetch('/Chat/Kontakte', { cache: 'no-store' })
            .then(function (r) { return r.ok ? r.json() : []; })
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
            })
            .catch(function () {});
    }

    function oeffneGespraech(kontakt) {
        aktiverKontakt = { id: kontakt.id, name: kontakt.name };
        el.titel.textContent = kontakt.name;
        el.kontakte.hidden = true;
        el.gespraech.hidden = false;
        el.zurueck.hidden = false;
        el.verlauf.innerHTML = '';
        angezeigteIds.clear();

        fetch('/Chat/Verlauf/' + kontakt.id, { cache: 'no-store' })
            .then(function (r) { return r.ok ? r.json() : []; })
            .then(function (nachrichten) {
                if (nachrichten.length === 0) {
                    const leer = document.createElement('div');
                    leer.className = 'sd-chat-leer';
                    leer.textContent = 'Noch keine Nachrichten. Schreib die erste!';
                    el.verlauf.appendChild(leer);
                }
                nachrichten.forEach(zeigeNachricht);
                return ladeUngelesen();
            })
            .catch(function () {});

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
        if (!n) return;
        if (n.id && angezeigteIds.has(n.id)) return;
        if (n.id) angezeigteIds.add(n.id);

        const leer = el.verlauf.querySelector('.sd-chat-leer');
        if (leer) leer.remove();

        const blase = document.createElement('div');
        blase.className = 'sd-chat-blase ' + (n.vonId === meineId ? 'sd-chat-ich' : 'sd-chat-du');

        const text = document.createElement('div');
        text.textContent = n.text;
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
        toastTimer = setTimeout(function () { el.toast.hidden = true; }, 4500);
    }

    function istFensterOffen() {
        return !el.fenster.hidden;
    }

    function initialen(name) {
        return name.split(' ').filter(Boolean).map(function (t) { return t[0]; }).join('').slice(0, 2).toUpperCase();
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

        el.text.value = '';
        el.text.focus();

        // Über /Chat/Senden schicken (speichert in DB + triggert SignalR-Push)
        fetch('/Chat/Senden', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                anId: aktiverKontakt.id,
                vonId: meineId,
                text: text
            })
        })
        .then(function (r) {
            return r.json().then(function (body) {
                if (!r.ok) throw new Error(body.fehler || 'Nachricht konnte nicht gesendet werden.');
                return body;
            });
        })
        .then(function (nachricht) {
            zeigeNachricht(nachricht);
        })
        .catch(function (err) {
            console.error(err);
            el.text.value = text; // Text wiederherstellen
            zeigeToast('Hinweis', err.message || 'Nachricht konnte nicht gesendet werden.');
        });
    });
})();
