"""Checks the running API in an isolated database. Python 3 standard library only."""
import json
import sys
from pathlib import Path
from urllib.request import Request, urlopen
from urllib.error import HTTPError

base = (sys.argv[1] if len(sys.argv) > 1 else 'http://localhost:5318').rstrip('/')
count = 0

def check(condition, message):
    global count
    if not condition: raise AssertionError(message)
    count += 1
    print(f'PASS {count:02}: {message}')

def request(path, method='GET', body=None):
    req = Request(base + path, data=None if body is None else json.dumps(body).encode(), method=method)
    if body is not None: req.add_header('Content-Type', 'application/json')
    try:
        with urlopen(req, timeout=15) as response: return response.status, response.read(), response.headers
    except HTTPError as error: return error.code, error.read(), error.headers

status, data, _ = request('/swagger/index.html')
check(status == 200 and b'id="swagger-ui"' in data, 'Swagger UI HTML is served')
for asset in ['/swagger/swagger-ui.css','/swagger/swagger-ui-bundle.js','/swagger/swagger-ui-standalone-preset.js','/swagger/index.js']:
    status, data, _ = request(asset)
    check(status == 200 and len(data) > 1000, f'Local Swagger UI asset {asset}')
status, data, _ = request('/swagger/v1/swagger.json')
check(status == 200, 'OpenAPI JSON is served')
document = json.loads(data)
expected = {'/': {'get'}, '/postings': {'get','post'}, '/postings/{id}': {'get','put','delete'}}
operations = [(path, method, value) for path, items in document['paths'].items() for method, value in items.items() if method in {'get','post','put','delete','patch'}]
check({p: {m for path,m,_ in operations if path == p} for p in document['paths']} == expected, 'All six original API operations are documented')
check(len({o['operationId'] for _,_,o in operations}) == 6, 'Operation names are unique')
for path, method, operation in operations:
    check(bool(operation.get('summary')) and bool(operation.get('description')), f'{method.upper()} {path} has a name and description')
    for parameter in operation.get('parameters', []):
        check(parameter['name'] == 'id' and parameter['in'] == 'path' and parameter['required'] and bool(parameter.get('description')), f'{method.upper()} id path parameter is described')
    if method in {'post','put'}:
        check(bool(operation['requestBody'].get('description')) and 'application/json' in operation['requestBody']['content'], f'{method.upper()} JSON request body is described')
    check(all(v.get('description') for v in operation['responses'].values()), f'{method.upper()} {path} response statuses are described')

schemas = document['components']['schemas']
for name, size in [('PostingGetDto',13),('PostingPostDto',10),('PostingPutDto',12)]:
    schema = schemas[name]
    check(bool(schema.get('description')) and len(schema['properties']) == size, f'{name} class and expected {size} properties are present')
    for field, definition in schema['properties'].items():
        check(bool(definition.get('description')), f'{name}.{field} has XML-derived documentation')
check(schemas['DeliveryType']['type'] == 'string' and schemas['DeliveryType']['enum'] == ['Department','Courier','ExpressCourier'], 'DeliveryType schema matches JSON string enum serialization')
check('id' not in schemas['PostingPostDto']['properties'] and 'price' not in schemas['PostingPostDto']['properties'], 'POST does not expose server-generated fields')
check('createdAt' not in schemas['PostingPutDto']['properties'], 'PUT does not expose immutable CreatedAt')

post = dict(from_='Kyiv', to='Lviv', content='Swagger check', deliveryType='Department', weight=2.5, width=20.5, height=15.25, depth=3, value=100.5, description='Temporary test record')
post['from'] = post.pop('from_')
created_id = None
try:
    status, data, headers = request('/postings','POST',post)
    saved = json.loads(data)
    created_id = saved.get('id')
    check(status == 201 and saved['price'] == 65 and headers.get('Location'), 'Documented POST returns 201, server price and Location')
    status, data, _ = request(f'/postings/{created_id}')
    check(status == 200 and json.loads(data)['content'] == post['content'], 'Documented GET by id returns saved DTO')
    put = {**post,'id':created_id,'price':72.5,'content':'Updated from OpenAPI test'}
    status, data, _ = request(f'/postings/{created_id}','PUT',put)
    check(status == 200 and json.loads(data)['price'] == 72.5 and json.loads(data)['createdAt'] == saved['createdAt'], 'Documented PUT updates data and preserves date')
    status, data, _ = request('/postings','POST',{**post,'weight':0})
    check(status == 400 and 'Weight' in json.loads(data)['errors'], 'Business validation returns documented 400 ValidationProblem')
    status, data, _ = request(f'/postings/{created_id}','PUT',{**put,'id':created_id+1})
    check(status == 400 and 'Id' in json.loads(data)['errors'], 'Mismatching PUT id returns 400')
    status, data, _ = request(f'/postings/{created_id}','DELETE')
    check(status == 204 and not data, 'Documented DELETE returns 204 with no body')
    status, _, _ = request(f'/postings/{created_id}')
    check(status == 404, 'GET of deleted record returns documented 404')
finally:
    if created_id is not None: request(f'/postings/{created_id}','DELETE')
print(f'All {count} checks passed.')
