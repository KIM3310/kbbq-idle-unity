.SHELLFLAGS := -eu -c
PYTHON ?= python3
VENV ?= .venv
VENV_PYTHON := $(VENV)/bin/python
VENV_STAMP := $(VENV)/.installed-dev

.PHONY: check-python install backend-test repository-verify pages-deploy verify

check-python:
	$(PYTHON) -c 'import sys; raise SystemExit(0 if sys.version_info >= (3, 11) else "Python 3.11+ is required")'

$(VENV_PYTHON): check-python
	@if [ ! -x "$(VENV_PYTHON)" ] || ! $(VENV_PYTHON) -c "import sys; raise SystemExit(0 if sys.version_info >= (3, 11) else 1)" >/dev/null 2>&1; then \
		rm -rf $(VENV); \
		$(PYTHON) -m venv $(VENV); \
	fi

$(VENV_STAMP): pyproject.toml $(VENV_PYTHON)
	$(VENV_PYTHON) -m pip install --upgrade pip
	$(VENV_PYTHON) -m pip install -e ".[dev]"
	touch $(VENV_STAMP)

install: $(VENV_STAMP)

backend-test: install
	$(VENV_PYTHON) -m pip check
	$(VENV_PYTHON) -m compileall -q server
	$(VENV_PYTHON) -m pytest -W error -q

repository-verify:
	$(PYTHON) scripts/validate_repository_surface.py
	$(PYTHON) scripts/validate_architecture_blueprint.py
	$(PYTHON) tools/validate_monetization_boundary.py

pages-deploy:
	npx --yes wrangler@latest pages deploy docs --project-name=kbbq-idle-unity --branch=main

verify: backend-test repository-verify
