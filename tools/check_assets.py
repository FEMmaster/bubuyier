"""Standard-library PNG dimension/structure check; requires no pip dependencies."""
import pathlib, struct, sys
root = pathlib.Path(__file__).resolve().parents[1] / 'assets' / 'characters'
errors=[]
for who in ('bubu','yier'):
    shapes={}
    for kind in ('body','blink','paw_left','paw_right'):
        p=root/f'{who}_{kind}.png'
        if not p.exists(): errors.append(f'Missing {p.name}');continue
        data=p.read_bytes()
        if len(data)<24 or data[:8]!=b'\x89PNG\r\n\x1a\n':errors.append(f'Invalid PNG {p.name}');continue
        shapes[kind]=struct.unpack('>II',data[16:24])
        print(p.name,shapes[kind],len(data),'bytes')
    if shapes.get('body') != shapes.get('blink'):
        errors.append(f'{who}: body/blink canvas dimensions differ')
print('NOTE: Equal canvas sizes do not guarantee pixel-perfect blink alignment.')
if errors:
    print('\n'.join(errors),file=sys.stderr)
    sys.exit(1)
