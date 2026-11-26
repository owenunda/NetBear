// Los mismos cuadros y reglas que src/NetBear/Sprites.cs y BearAnimator.cs, dibujados en <canvas>.
const E = "................";
const BODY = [
  E, E, E,
  "..........OO.OO.",
  ".........OBBOBBO",
  "..OOOOOOOBBBBBBO",
  ".OBBBBBBBBBBBEBO",
  "OBBBBBBBBBBBBLNO",
  "OBBBBBBBBBBBLLLO",
  "OBBBBBBBBBBBOOO.",
  ".OBBBBBBBBBBO...",
  ".OBBOOOOOOBBO...",
];
const BLINK_BODY = [...BODY.slice(0, 6), ".OBBBBBBBBBBBBBO", ...BODY.slice(7)];
const legs = (body, a, b) => [...body, a, b, E, E];

const RUNNING = [
  legs(BODY, "OBO........OBO..", "OO..........OO.."),
  legs(BODY, "OBO......OBO....", "OO.......OO....."),
  legs(BODY, "...OBO..OBO.....", "....OO..OO......"),
  legs(BODY, "..OBO....OBO....", "..OO.....OO....."),
  legs(BODY, "..OBO......OBO..", "..OO........OO.."),
];
const STANDING = legs(BODY, "..OBO.....OBO...", "..OOO.....OOO...");
const BLINKING = legs(BLINK_BODY, "..OBO.....OBO...", "..OOO.....OOO...");
const IDLE = [...Array(11).fill(STANDING), BLINKING];
const SLEEPING = [
  E, "..........OO.OO.", "...OOOOOOOBBBBBO", "..OBBBBBBBBBBOBO", ".OBBBBBBBBBBBLNO",
  ".OBBBBBBBBBBBLLO", ".OOOOOOOOOOOOOO.", E, E, E,
];
const HIBERNATING = [
  [E, E, E, E, E, "............Z...", ...SLEEPING],
  [E, "...........ZZZ..", "............Z...", "...........ZZZ..", E, E, ...SLEEPING],
];

// Paleta de la barra de tareas oscura.
const PALETTE = { O: "#f2e6d8", B: "#a0673a", L: "#e8c9a0", E: "#1a1a1a", N: "#1a1a1a", Z: "#ffffff" };

const RUN_THRESHOLD = 10 * 1024;
const MAX_SPEED = 10 * 1024 * 1024;

function stateFor(connected, bps) {
  if (!connected) return "hibernating";
  return bps < RUN_THRESHOLD ? "idle" : "running";
}

function intervalFor(state, bps) {
  if (state === "hibernating") return 500;
  if (state === "idle") return 250;
  const c = Math.min(Math.max(bps, RUN_THRESHOLD), MAX_SPEED);
  const t = Math.log(c / RUN_THRESHOLD) / Math.log(MAX_SPEED / RUN_THRESHOLD);
  return Math.round(200 + (40 - 200) * t);
}

const FRAMES = { running: RUNNING, idle: IDLE, hibernating: HIBERNATING };

export function formatSpeed(bps) {
  if (!(bps >= 1024)) return `${Math.floor(Math.max(0, bps))} B/s`;
  const units = ["KB/s", "MB/s", "GB/s"];
  let v = bps / 1024, u = 0;
  while (Math.round(v * 10) / 10 >= 1024 && u < units.length - 1) { v /= 1024; u++; }
  return `${v.toFixed(1)} ${units[u]}`;
}

function draw(ctx, frame) {
  ctx.clearRect(0, 0, 16, 16);
  frame.forEach((row, y) => {
    for (let x = 0; x < row.length; x++) {
      const c = PALETTE[row[x]];
      if (!c) continue;
      ctx.fillStyle = c;
      ctx.fillRect(x, y, 1, 1);
    }
  });
}

const still = matchMedia("(prefers-reduced-motion: reduce)");

// Un oso que se anima en uno o varios <canvas width=16 height=16>.
export function bear(canvases, { connected = true, bps = 0 } = {}) {
  const ctxs = canvases.map((c) => c.getContext("2d"));
  let state = stateFor(connected, bps), frame = 0, timer;

  function tick() {
    const frames = FRAMES[state];
    frame %= frames.length;
    ctxs.forEach((ctx) => draw(ctx, frames[frame]));
    if (still.matches) return;
    frame++;
    timer = setTimeout(tick, intervalFor(state, bps));
  }

  tick();
  return {
    set(next) {
      connected = next.connected ?? connected;
      bps = next.bps ?? bps;
      const s = stateFor(connected, bps);
      if (s !== state) { state = s; frame = 0; clearTimeout(timer); tick(); }
      return state;
    },
  };
}
