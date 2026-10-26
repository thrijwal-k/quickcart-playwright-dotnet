// Waits until every URL passed on the command line answers with HTTP 2xx (used by CI before running tests).
const urls = process.argv.slice(2);
const deadline = Date.now() + 90_000;

for (const url of urls) {
  for (;;) {
    try {
      const res = await fetch(url);
      if (res.ok) break;
    } catch {
      // server not up yet
    }
    if (Date.now() > deadline) {
      console.error(`Timed out waiting for ${url}`);
      process.exit(1);
    }
    await new Promise((resolve) => setTimeout(resolve, 1000));
  }
  console.log(`Ready: ${url}`);
}
