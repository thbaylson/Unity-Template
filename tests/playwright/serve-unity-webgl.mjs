import { createServer } from "node:http";
import { createReadStream, existsSync, statSync } from "node:fs";
import path from "node:path";

const contentTypes = new Map([
  [".css", "text/css; charset=utf-8"],
  [".data", "application/octet-stream"],
  [".html", "text/html; charset=utf-8"],
  [".ico", "image/x-icon"],
  [".js", "text/javascript; charset=utf-8"],
  [".json", "application/json; charset=utf-8"],
  [".png", "image/png"],
  [".svg", "image/svg+xml"],
  [".wasm", "application/wasm"]
]);

const compressionExtensions = new Map([
  [".br", "br"],
  [".gz", "gzip"]
]);

function parseArguments(argv) {
  let rootDirectory = path.resolve(process.cwd(), "Build/WebGL");
  let port = 4173;

  for (let index = 0; index < argv.length; index += 1) {
    const currentArgument = argv[index];
    if (currentArgument === "--root" && argv[index + 1]) {
      rootDirectory = path.resolve(process.cwd(), argv[index + 1]);
      index += 1;
      continue;
    }

    if (currentArgument === "--port" && argv[index + 1]) {
      port = Number.parseInt(argv[index + 1], 10);
      index += 1;
    }
  }

  return { rootDirectory, port };
}

function getContentType(filePath) {
  let effectivePath = filePath;
  const compressionExtension = path.extname(filePath).toLowerCase();
  if (compressionExtensions.has(compressionExtension)) {
    effectivePath = filePath.slice(0, -compressionExtension.length);
  }

  return contentTypes.get(path.extname(effectivePath).toLowerCase()) ?? "application/octet-stream";
}

function resolveFilePath(rootDirectory, requestPath) {
  const normalizedRequestPath = requestPath === "/" ? "/index.html" : requestPath;
  const resolvedPath = path.resolve(rootDirectory, `.${normalizedRequestPath}`);
  const relativePath = path.relative(rootDirectory, resolvedPath);

  if (relativePath.startsWith("..") || path.isAbsolute(relativePath)) {
    return null;
  }

  return resolvedPath;
}

const { rootDirectory, port } = parseArguments(process.argv.slice(2));

if (!existsSync(rootDirectory)) {
  console.error(`Unity WebGL build directory was not found at ${rootDirectory}.`);
  process.exit(1);
}

const server = createServer((request, response) => {
  const requestUrl = new URL(request.url ?? "/", `http://${request.headers.host ?? "127.0.0.1"}`);
  const filePath = resolveFilePath(rootDirectory, requestUrl.pathname);
  if (!filePath || !existsSync(filePath) || statSync(filePath).isDirectory()) {
    response.writeHead(404, { "Content-Type": "text/plain; charset=utf-8" });
    response.end("Not found");
    return;
  }

  const headers = {
    "Cache-Control": "no-store",
    "Content-Length": statSync(filePath).size,
    "Content-Type": getContentType(filePath)
  };

  const compressionExtension = path.extname(filePath).toLowerCase();
  if (compressionExtensions.has(compressionExtension)) {
    headers["Content-Encoding"] = compressionExtensions.get(compressionExtension);
  }

  response.writeHead(200, headers);

  if (request.method === "HEAD") {
    response.end();
    return;
  }

  createReadStream(filePath).pipe(response);
});

server.listen(port, "127.0.0.1", () => {
  console.log(`Serving Unity WebGL build from ${rootDirectory} at http://127.0.0.1:${port}`);
});
