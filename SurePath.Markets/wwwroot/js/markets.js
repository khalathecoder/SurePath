// ============================================================
// SUREPATH MARKETS — markets.js
// Trade Journal Logic + Ticker Simulation + UI Helpers
// ============================================================

// ============================================================
// STATE
// ============================================================
let trades = JSON.parse(localStorage.getItem('sp_trades') || '[]');
let currentDirection = 'long';
let selectedEmotions = [];

const QUOTES = [
    { q: "The goal of a successful trader is to make the best trades. Money is secondary.", a: "Alexander Elder" },
    { q: "Plan the trade, trade the plan.", a: "Trading Maxim" },
    { q: "Risk comes from not knowing what you're doing.", a: "Warren Buffett" },
    { q: "The market is a device for transferring money from the impatient to the patient.", a: "Warren Buffett" },
    { q: "Cut your losses quickly and let your profits run.", a: "Trading Maxim" },
    { q: "It's not whether you're right or wrong that's important, but how much money you make when you're right.", a: "George Soros" },
    { q: "The elements of good trading are: cutting losses, cutting losses, and cutting losses.", a: "Ed Seykota" },
    { q: "I just wait until there is money lying in the corner, and all I have to do is go over there and pick it up.", a: "Jim Rogers" },
];

// ============================================================
// INIT
// ============================================================
window.onload = function () {
    const now = new Date();

    // Set current date display
    const dateEl = document.getElementById('current-date');
    if (dateEl) {
        dateEl.textContent = now.toLocaleDateString('en-US', {
            weekday: 'long', month: 'long', day: 'numeric', year: 'numeric'
        });
    }

    // Set default trade date to today
    const dateInput = document.getElementById('f-date');
    if (dateInput) {
        dateInput.value = now.toISOString().split('T')[0];
    }

    // Random quote
    const q = QUOTES[Math.floor(Math.random() * QUOTES.length)];
    const quoteEl = document.getElementById('quote-text');
    const authorEl = document.getElementById('quote-author');
    if (quoteEl) quoteEl.textContent = '"' + q.q + '"';
    if (authorEl) authorEl.textContent = '— ' + q.a;

    // Start status bar clock
    updateStatusTime();
    setInterval(updateStatusTime, 1000);

    // Start ticker simulation
    setInterval(tickerUpdate, 3000);

    // Render existing trades from localStorage
    renderAll();
};

// ============================================================
// STATUS BAR CLOCK
// ============================================================
function updateStatusTime() {
    const el = document.getElementById('status-time');
    if (el) {
        el.textContent = new Date().toLocaleTimeString('en-US', { hour12: false }) + ' CT';
    }
}

// ============================================================
// DIRECTION TOGGLE
// ============================================================
function setDir(dir, btn) {
    currentDirection = dir;
    const hidden = document.getElementById('f-direction');
    if (hidden) hidden.value = dir;

    document.querySelectorAll('.dir-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    calcResults();
}

// ============================================================
// EMOTION TOGGLE
// ============================================================
function toggleEmotion(btn) {
    const label = btn.textContent.trim();
    if (selectedEmotions.includes(label)) {
        selectedEmotions = selectedEmotions.filter(e => e !== label);
        btn.style.borderColor = '';
        btn.style.color = '';
        btn.style.background = '';
    } else {
        selectedEmotions.push(label);
        btn.style.borderColor = 'var(--green)';
        btn.style.color = 'var(--green)';
        btn.style.background = 'rgba(0,214,143,0.08)';
    }
}

// ============================================================
// REAL-TIME CALCULATIONS
// ============================================================
function calcResults() {
    const entry = parseFloat(document.getElementById('f-entry')?.value) || 0;
    const exit = parseFloat(document.getElementById('f-exit')?.value) || 0;
    const stop = parseFloat(document.getElementById('f-stop')?.value) || 0;
    const target = parseFloat(document.getElementById('f-target')?.value) || 0;
    const size = parseFloat(document.getElementById('f-size')?.value) || 1;
    const dir = currentDirection;

    let pnl = 0, risk = 0, rr = null, pct = 0;

    if (entry > 0) {
        if (exit > 0) {
            pnl = dir === 'long' ? (exit - entry) * size : (entry - exit) * size;
            pct = dir === 'long' ? ((exit - entry) / entry * 100) : ((entry - exit) / entry * 100);
        }
        if (stop > 0) {
            risk = dir === 'long' ? (entry - stop) * size : (stop - entry) * size;
        }
        if (stop > 0 && target > 0) {
            const riskPts = Math.abs(entry - stop);
            const rewardPts = Math.abs(target - entry);
            rr = riskPts > 0 ? (rewardPts / riskPts) : null;
        }
    }

    const pnlEl = document.getElementById('res-pnl');
    if (pnlEl) {
        pnlEl.textContent = pnl !== 0 ? '$' + pnl.toFixed(2) : '—';
        pnlEl.className = 'result-value' + (pnl > 0 ? ' green' : pnl < 0 ? ' red' : '');
    }

    const rrEl = document.getElementById('res-rr');
    if (rrEl) rrEl.textContent = rr !== null ? '1:' + rr.toFixed(2) : '—';

    const riskEl = document.getElementById('res-risk');
    if (riskEl) riskEl.textContent = risk > 0 ? '$' + risk.toFixed(2) : '—';

    const pctEl = document.getElementById('res-pct');
    if (pctEl) {
        pctEl.textContent = pct !== 0 ? pct.toFixed(2) + '%' : '—';
        pctEl.className = 'result-value' + (pct > 0 ? ' green' : pct < 0 ? ' red' : '');
    }
}

// ============================================================
// ADD TRADE
// ============================================================
function addTrade() {
    const sym = document.getElementById('f-symbol')?.value.trim().toUpperCase();
    const date = document.getElementById('f-date')?.value;
    const dir = currentDirection;
    const type = document.getElementById('f-type')?.value;
    const entry = parseFloat(document.getElementById('f-entry')?.value);
    const exit = parseFloat(document.getElementById('f-exit')?.value) || null;
    const stop = parseFloat(document.getElementById('f-stop')?.value) || null;
    const target = parseFloat(document.getElementById('f-target')?.value) || null;
    const size = parseFloat(document.getElementById('f-size')?.value) || 1;
    const status = document.getElementById('f-status')?.value;
    const setup = document.getElementById('f-setup')?.value.trim();
    const tf = document.getElementById('f-tf')?.value;
    const notes = document.getElementById('f-notes')?.value.trim();

    if (!sym || !date || isNaN(entry)) {
        alert('Please fill in Symbol, Date, and Entry Price at minimum.');
        return;
    }

    let pnl = null, rr = null;
    if (exit !== null && !isNaN(exit)) {
        pnl = dir === 'long' ? (exit - entry) * size : (entry - exit) * size;
    }
    if (stop !== null && target !== null) {
        const riskPts = Math.abs(entry - stop);
        const rewardPts = Math.abs(target - entry);
        rr = riskPts > 0 ? rewardPts / riskPts : null;
    }

    const trade = {
        id: Date.now(),
        sym, date, dir, type, entry, exit, stop, target,
        size, status, setup, tf, notes,
        emotions: [...selectedEmotions],
        pnl, rr,
        createdAt: new Date().toISOString()
    };

    trades.unshift(trade);
    localStorage.setItem('sp_trades', JSON.stringify(trades));
    clearForm();
    renderAll();
}

// ============================================================
// CLEAR FORM
// ============================================================
function clearForm() {
    ['f-symbol', 'f-entry', 'f-exit', 'f-stop', 'f-target', 'f-setup', 'f-notes'].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.value = '';
    });

    const sizeEl = document.getElementById('f-size');
    if (sizeEl) sizeEl.value = '10';

    const statusEl = document.getElementById('f-status');
    if (statusEl) statusEl.value = 'open';

    // Reset emotions
    selectedEmotions = [];
    document.querySelectorAll('.btn-ghost').forEach(b => {
        b.style.borderColor = '';
        b.style.color = '';
        b.style.background = '';
    });

    // Reset direction to long
    const longBtn = document.querySelector('.dir-btn.long');
    if (longBtn) setDir('long', longBtn);

    // Reset result panel
    ['res-pnl', 'res-rr', 'res-risk', 'res-pct'].forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            el.textContent = '—';
            el.className = 'result-value';
        }
    });
}

// ============================================================
// DELETE TRADE
// ============================================================
function deleteTrade(id) {
    trades = trades.filter(t => t.id !== id);
    localStorage.setItem('sp_trades', JSON.stringify(trades));
    renderAll();
}

// ============================================================
// RENDER ALL
// ============================================================
function renderAll() {
    renderTable();
    renderRecent();
    updateStats();

    const countEl = document.getElementById('trade-count');
    if (countEl) {
        countEl.textContent = trades.length + ' trade' + (trades.length !== 1 ? 's' : '');
    }
}

function renderTable() {
    const wrap = document.getElementById('trade-log-wrap');
    if (!wrap) return;

    if (trades.length === 0) {
        wrap.innerHTML = `
            <div class="empty-state">
                <div class="empty-icon">📋</div>
                <div>No trades logged yet.</div>
                <div style="margin-top:4px; font-size:11px;">Fill out the form above and hit Log Trade.</div>
            </div>`;
        return;
    }

    wrap.innerHTML = `
        <table class="trade-table">
            <thead>
                <tr>
                    <th>Symbol</th>
                    <th>Date</th>
                    <th>Dir</th>
                    <th>Entry</th>
                    <th>Exit</th>
                    <th>Size</th>
                    <th>P&L</th>
                    <th>R:R</th>
                    <th>Setup</th>
                    <th>Status</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                ${trades.map(t => `
                    <tr>
                        <td style="font-weight:500; color:#fff;">${t.sym}</td>
                        <td>${t.date}</td>
                        <td><span class="badge badge-${t.dir}">${t.dir.toUpperCase()}</span></td>
                        <td>${t.entry.toFixed(2)}</td>
                        <td>${t.exit !== null && !isNaN(t.exit) ? t.exit.toFixed(2) : '—'}</td>
                        <td>${t.size}</td>
                        <td class="${t.pnl !== null ? (t.pnl >= 0 ? 'pnl-pos' : 'pnl-neg') : ''}">
                            ${t.pnl !== null ? '$' + t.pnl.toFixed(2) : '—'}
                        </td>
                        <td style="color:var(--gold)">${t.rr !== null ? '1:' + t.rr.toFixed(2) : '—'}</td>
                        <td style="color:var(--text-dim); font-size:11px;">${t.setup || '—'}</td>
                        <td><span class="badge badge-${t.status}">${t.status}</span></td>
                        <td>
                            <button onclick="deleteTrade(${t.id})"
                                style="background:none; border:none; color:var(--text-mute); cursor:pointer; font-size:16px; padding:2px 6px;"
                                title="Remove">×</button>
                        </td>
                    </tr>
                `).join('')}
            </tbody>
        </table>`;
}

function renderRecent() {
    const el = document.getElementById('recent-trades');
    if (!el) return;

    if (trades.length === 0) {
        el.innerHTML = `<div style="color:var(--text-mute); font-size:11px; padding:8px 0;">No entries yet.</div>`;
        return;
    }

    el.innerHTML = trades.slice(0, 5).map(t => `
        <div class="mini-trade">
            <div>
                <div class="mini-trade-sym">
                    ${t.sym}
                    <span style="color:var(--text-dim); font-size:10px;">${t.dir.toUpperCase()}</span>
                </div>
                <div class="mini-trade-meta">${t.date} · ${t.tf || ''}</div>
            </div>
            <div class="mini-pnl ${t.pnl !== null ? (t.pnl >= 0 ? 'pnl-pos' : 'pnl-neg') : ''}">
                ${t.pnl !== null ? '$' + t.pnl.toFixed(2) : '—'}
            </div>
        </div>
    `).join('');
}

function updateStats() {
    const total = trades.reduce((s, t) => s + (t.pnl || 0), 0);
    const closed = trades.filter(t => t.status === 'closed' && t.pnl !== null);
    const wins = closed.filter(t => t.pnl > 0).length;
    const wr = closed.length > 0 ? (wins / closed.length * 100) : 0;
    const rrVals = trades.filter(t => t.rr !== null).map(t => t.rr);
    const avgRR = rrVals.length > 0 ? rrVals.reduce((a, b) => a + b, 0) / rrVals.length : null;

    const totalEl = document.getElementById('stat-total-pnl');
    if (totalEl) {
        totalEl.textContent = '$' + total.toFixed(2);
        totalEl.className = 'stat-card-value ' + (total >= 0 ? 'green' : 'red');
    }

    const wrEl = document.getElementById('stat-winrate');
    if (wrEl) wrEl.textContent = wr.toFixed(0) + '%';

    const barEl = document.getElementById('stat-winbar');
    if (barEl) barEl.style.width = wr + '%';

    const trEl = document.getElementById('stat-trades');
    if (trEl) trEl.textContent = trades.length;

    const rrEl = document.getElementById('stat-rr');
    if (rrEl) rrEl.textContent = avgRR !== null ? '1:' + avgRR.toFixed(2) : '—';
}

// ============================================================
// CHECKLIST
// ============================================================
function toggleCheck(el) {
    el.classList.toggle('checked');
    const box = el.querySelector('.check-box');
    if (box) box.textContent = el.classList.contains('checked') ? '✓' : '';
}

// ============================================================
// SIMULATED TICKER UPDATES
// ============================================================
function nudge(val, spread) {
    return (val + (Math.random() - 0.5) * spread).toFixed(2);
}

function tickerUpdate() {
    const esEl = document.getElementById('t-es');
    const nqEl = document.getElementById('t-nq');
    const clEl = document.getElementById('t-cl');

    if (!esEl || !nqEl || !clEl) return;

    const es = parseFloat(esEl.textContent.replace(/,/g, ''));
    const nq = parseFloat(nqEl.textContent.replace(/,/g, ''));
    const cl = parseFloat(clEl.textContent);

    esEl.textContent = parseFloat(nudge(es, 2)).toLocaleString('en-US', { minimumFractionDigits: 2 });
    nqEl.textContent = parseFloat(nudge(nq, 8)).toLocaleString('en-US', { minimumFractionDigits: 2 });
    clEl.textContent = nudge(cl, 0.15);
}