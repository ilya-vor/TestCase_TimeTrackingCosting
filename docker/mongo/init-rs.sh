#!/bin/bash
set -e

# Инициализация однонодового replica set "rs0" — нужен для транзакций MongoDB.
# Скрипт идемпотентен: если реплика-сет уже инициализирован, повторный запуск ничего не делает.

for i in $(seq 1 60); do
  if mongosh --host mongo:27017 --quiet --eval "db.runCommand({ping:1}).ok" >/dev/null 2>&1; then
    break
  fi
  sleep 1
done

mongosh --host mongo:27017 --quiet <<'EOF'
try {
  rs.status();
  print("replica set already initialized");
} catch (e) {
  rs.initiate({ _id: "rs0", members: [ { _id: 0, host: "mongo:27017" } ] });
  print("replica set initiated");
}
EOF

# Ждём, пока нода станет primary — только после этого стартует API.
for i in $(seq 1 60); do
  if mongosh --host mongo:27017 --quiet --eval "try{db.hello().isWritablePrimary}catch(e){false}" | grep -q true; then
    echo "mongo is primary"
    exit 0
  fi
  sleep 1
done

echo "mongo did not become primary" >&2
exit 1
